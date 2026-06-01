#!/usr/bin/env python3
"""Malcha 교차 테스트 — catalog 이미지 배치 inference → JSON 출력"""
import argparse
import json
import os
import sys

import numpy as np


def win_to_wsl(path: str) -> str:
    if len(path) >= 2 and path[1] == ':':
        return '/mnt/' + path[0].lower() + path[2:].replace('\\', '/')
    return path.replace('\\', '/')


def load_config():
    """mycar cwd + donkeycar.load_config() (train.py와 동일)."""
    try:
        import donkeycar as dk
        return dk.load_config()
    except Exception:
        pass
    try:
        import config as mycar_config
        if hasattr(mycar_config, 'cfg'):
            return mycar_config.cfg
    except Exception:
        pass

    class Cfg:
        IMAGE_W = 160
        IMAGE_H = 120
        IMAGE_DEPTH = 3
        DEFAULT_MODEL_TYPE = 'linear'
    return Cfg()


def load_pilot(model_path, cfg, model_type=None):
    """학습 시 모델 타입(linear/categorical 등)에 맞는 pilot 로드."""
    from donkeycar.utils import get_model_by_type

    errors = []
    candidates = []
    for t in (model_type, getattr(cfg, 'DEFAULT_MODEL_TYPE', None), 'linear', 'categorical'):
        if t and t not in candidates:
            candidates.append(t)

    for mt in candidates:
        try:
            kl = get_model_by_type(mt, cfg)
            kl.load(model_path)
            return PilotWrapper(mt, kl)
        except Exception as ex:
            errors.append(f'{mt}: {ex}')

    try:
        import tensorflow as tf
        model = tf.keras.models.load_model(model_path, compile=False)
        return PilotWrapper('raw', model)
    except Exception as ex:
        errors.append(f'tf.keras: {ex}')

    raise RuntimeError('모델 로드 실패:\n' + '\n'.join(errors))


class PilotWrapper:
    def __init__(self, kind, obj):
        self.kind = kind
        self.obj = obj

    def predict(self, img_arr):
        if self.kind != 'raw':
            out = self._run_donkey(img_arr)
            return _parse_output(out)

        from donkeycar.parts.keras import normalize_image
        norm = normalize_image(img_arr) if img_arr.dtype == np.uint8 else img_arr
        batch = norm.reshape((1,) + norm.shape)
        raw = self.obj.predict(batch, verbose=0)
        return _parse_raw_keras(raw)

    def _run_donkey(self, img_arr):
        pilot = self.obj
        for fn in (lambda: pilot.run(img_arr), lambda: pilot.inference(img_arr, None)):
            try:
                return fn()
            except TypeError:
                try:
                    return pilot.inference(img_arr)
                except Exception:
                    continue
            except AttributeError:
                continue
        raise RuntimeError('donkey pilot run/inference 실패')

    def input_size(self, cfg):
        if self.kind == 'raw':
            shape = getattr(self.obj, 'input_shape', None)
            if shape and len(shape) >= 4:
                return int(shape[2]), int(shape[1])
        return int(getattr(cfg, 'IMAGE_W', 160)), int(getattr(cfg, 'IMAGE_H', 120))


def _parse_raw_keras(raw):
    try:
        import donkeycar as dk
        if isinstance(raw, (list, tuple)) and len(raw) >= 2:
            angle_part, throttle_part = raw[0], raw[1]
            if hasattr(angle_part, 'shape') and len(angle_part.shape) >= 2 and angle_part.shape[-1] > 1:
                angle = float(dk.utils.linear_unbin(angle_part)[0])
            else:
                angle = float(angle_part[0][0] if hasattr(angle_part[0], '__len__') else angle_part[0])
            if hasattr(throttle_part, 'shape') and len(throttle_part.shape) >= 2 and throttle_part.shape[-1] > 1:
                n = throttle_part.shape[-1]
                throttle = float(dk.utils.linear_unbin(throttle_part, N=n, offset=0.0, R=0.5)[0])
            else:
                throttle = float(throttle_part[0][0] if hasattr(throttle_part[0], '__len__') else throttle_part[0])
            return angle, throttle
    except Exception:
        pass

    if isinstance(raw, (list, tuple)) and len(raw) >= 2:
        a = raw[0][0][0] if hasattr(raw[0][0], '__len__') else raw[0][0]
        t = raw[1][0][0] if hasattr(raw[1][0], '__len__') else raw[1][0]
        return float(a), float(t)
    return float(raw[0]), 0.0


def _parse_output(out):
    if isinstance(out, dict):
        angle = out.get('angle', out.get('user/angle', 0.0))
        throttle = out.get('throttle', out.get('user/throttle', 0.0))
        return float(angle), float(throttle)
    if isinstance(out, (list, tuple)):
        if len(out) >= 2:
            return float(out[0]), float(out[1])
        if len(out) == 1:
            return float(out[0]), 0.0
    return float(out), 0.0


def preprocess_image(path, cfg):
    """train.py TubDataset과 동일 — donkeycar.utils.load_image."""
    from donkeycar.utils import load_image
    return load_image(path, cfg)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--model', required=True, help='models/*.h5 (mycar cwd 기준)')
    ap.add_argument('--manifest', required=True)
    ap.add_argument('--output', required=True)
    ap.add_argument('--model-type', default='', help='linear|categorical (database.json Type)')
    args = ap.parse_args()

    model_path = args.model
    if not os.path.isabs(model_path):
        model_path = os.path.abspath(model_path)
    if not os.path.isfile(model_path):
        raise FileNotFoundError(f'모델 없음: {model_path}')

    with open(args.manifest, encoding='utf-8') as f:
        manifest = json.load(f)

    cfg = load_config()
    model_type = (args.model_type or manifest.get('modelType') or '').strip() or None
    pilot = load_pilot(model_path, cfg, model_type)

    results = []
    errors = []
    for item in manifest.get('frames', []):
        idx = int(item['index'])
        img_path = win_to_wsl(item['image'])
        if not os.path.isfile(img_path):
            errors.append({'index': idx, 'error': f'image not found: {img_path}'})
            continue
        try:
            arr = preprocess_image(img_path, cfg)
            angle, throttle = pilot.predict(arr)
            results.append({'index': idx, 'angle': angle, 'throttle': throttle})
        except Exception as ex:
            errors.append({'index': idx, 'error': str(ex)})

    payload = {
        'model': manifest.get('model', ''),
        'modelType': model_type or getattr(cfg, 'DEFAULT_MODEL_TYPE', ''),
        'predictions': results,
        'errors': errors,
    }
    out_dir = os.path.dirname(os.path.abspath(args.output))
    if out_dir:
        os.makedirs(out_dir, exist_ok=True)
    with open(args.output, 'w', encoding='utf-8') as f:
        json.dump(payload, f)

    angles = [r['angle'] for r in results]
    summary = {
        'ok': len(results) > 0,
        'count': len(results),
        'errors': len(errors),
        'pilot': pilot.kind,
        'angleMin': min(angles) if angles else None,
        'angleMax': max(angles) if angles else None,
    }
    print(json.dumps(summary))
    sys.exit(0 if results else 1)


if __name__ == '__main__':
    main()
