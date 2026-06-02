# 🚗 Malcha (말차) - 자율주행 동키카 학습 데이터 정제 및 관리 시스템

> **"C# WinForm과 WSL을 연동한 대용량 자율주행 학습 데이터 정제 및 Time-Machine 백업 복구 플랫폼"**

[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)]()
[![Python](https://img.shields.io/badge/Python-3776AB?style=for-the-badge&logo=python&logoColor=white)]()
[![Linux](https://img.shields.io/badge/Linux-FCC624?style=for-the-badge&logo=linux&logoColor=black)]()
<br>

## 📌 1. 프로젝트 배경 및 목표 (Motivation & Goal)
* **배경:** 기존 자율주행 동키카(DonkeyCar) 시스템은 주행 데이터(Tub/Catalog)에 섞인 노이즈(정지 상태, 쓰레기 데이터)를 정제하기 어렵고, 딥러닝 학습 과정이 리눅스 CLI 환경에 종속되어 있어 직관적인 조작이 불가능했습니다.
* **목표:** 사용자 친화적인 C# WinForm UI를 통해 수만 장의 프레임과 이미지를 시각적으로 모니터링하고, 논리적이고 안전하게 정제(Filter)하며, 윈도우 환경에서 클릭 한 번으로 WSL(리눅스 서브시스템)과 연동해 자율주행 모델을 학습시킬 수 있는 통합 툴 생성을 목표로 합니다.

<br>

## 🏗️ 2. 시스템 설계 및 아키텍처 (Architecture & SE)

### 2.1 핵심 아키텍처 패턴 (MVC 기반 Layered Architecture)
Malcha 프로젝트는 UI(View), 조율자(Controller), 비즈니스 로직(Service/Domain)을 철저히 분리하여 설계되었습니다.
* **Controller**: `CatalogEditorController`, `TrainingController` (화면 제어 및 흐름 통제)
* **Service**: `CatalogService`, `WslDataSyncService` (프레임 병합, 정제, WSL 명령 전달)
* **Model/Data**: 인메모리 스냅샷(Undo), `CatalogMerger` 등 순수 도메인 로직 처리

### 2.2 핵심 유스케이스 (Core Use Cases)
1. **[다중 카탈로그 병합 및 시각화]**: 폴더 형태의 N개의 분할된 데이터(catalog_0, catalog_1)를 `merged_final`이라는 단일 마스터 카탈로그로 병합 후, 차트(조향/쓰로틀)와 타임라인으로 랜더링.
2. **[Time-Machine 스냅샷 백업 및 롤백 복구]**: 필터 정제 시 발생하는 데이터 손실을 막기 위해 가벼운 인메모리(RAM) Undo(`Ctrl+Z`)를 제공하고, 명시적 저장 시 하드디스크에 시간별 백업을 저장. 사용자는 언제든 다이얼로그를 통해 "과거 시점"을 지정하여 즉각 복구 가능.
3. **[WSL 연동 데이터 훈련]**: 윈도우 환경에서 버튼 하나로 WSL 환경의 파이썬 스크립트(`train.py`)를 호출하여 AI 모델을 훈련시키고 실시간으로 Loss 그래프를 확인.

<br>

## ✨ 3. 주요 기능 (Key Features)

* **프레임 정제 필터 (Frame Refinement Filter)**
  * 주행 중 의미 없는 데이터(쓰로틀이 너무 낮거나 조향이 없는 정지 프레임 등)를 감지하여 일괄 삭제하는 알고리즘 제공.
* **시점 선택 복구 시스템 (Time-Machine Recovery)**
  * 백업 파일 목록을 스캔하여 사용자가 직접 다이얼로그에서 과거 특정 시점(ex. 2026-05-30 오전 10:00 백업본)을 선택해 안전하게 통째로 롤백할 수 있는 강력한 기능.
* **WSL 실시간 학습 진행률 로깅**
  * WSL 파이썬 프로세스를 호출한 뒤, 파이프(Pipe) 통신을 통해 Epoch 별 손실률(Loss) 로그를 실시간으로 파싱하여 WinForm 차트(`TrainingEpochDisplay`)에 아날로그하게 그려줌.

<br>

## 🛠️ 4. 트러블슈팅 및 기술적 의사결정 (Trouble Shooting)

### 🚨 Issue 1: 대용량 데이터 필터링 시 UI 프리징 및 I/O 오버헤드 문제
* **원인:** 초기에는 필터를 돌리거나 프레임을 1장 삭제할 때마다 즉시 디스크에 Auto-backup을 남기고 파일을 덮어쓰도록 설계하여 병목(I/O 프리징) 발생.
* **해결 과정:**
  1. 저장 전까지는 순수하게 인메모리(RAM) 스택 자료구조 위주로 스냅샷을 쌓는 시스템(Undo)을 메인으로 사용.
  2. 디스크 백업은 사용자가 명시적으로 '저장' 버튼을 누를 때 1회만 스냅샷을 생성하도록 백업 정책 변경.
* **결과:** 사용자 체감 속도 비약적 향상 및 디스크 수명(SSD 쓰기 양) 보호, 쓸모없는 찌꺼기 백업 파일 양산 방지.

### 🚨 Issue 2: 과거 데이터와 현재 데이터의 혼종(Merge) 발생 문제
* **원인:** 초기 백업 복구 로직(`CatalogMerger`)이 현재 망가진 데이터와 옛날 백업 데이터를 비교하여 빈 자리를 채워 넣는(Merge) 병합 방식으로 동작하여, 돌아가고 싶은 과거 시점 100% 그대로 롤백되지 않음.
* **해결 과정:** 과거 데이터와 현재 데이터를 섞어버리는 `MergeWithBackupAsync` 대신, 사용자가 [복구] 시점을 선택하면 해당 옛날 파일 자체를 `LoadCatalogFileAsync`로 읽어와 메모리에 통째로 '덮어씌우는(Overwrite)' 로직으로 재설계.
* **결과:** 소프트웨어 공학의 Undo/Redo 철학에 맞는 올바르고 안전한 '시점 선택 롤백' 달성.