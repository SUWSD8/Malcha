namespace Malcha.UI
{
    // ListBox 마우스 드래그 구간 선택 공통 유틸
    internal static class ListBoxDragSelectHelper
    {
        // 클릭 위치 → 항목 인덱스 (목록 위·아래도 끝 항목으로 클램프)
        public static int IndexFromPointClamped(ListBox listBox, Point clientPoint)
        {
            if (listBox.Items.Count == 0) return -1;

            int idx = listBox.IndexFromPoint(clientPoint);
            if (idx >= 0) return idx;

            for (int i = 0; i < listBox.Items.Count; i++)
            {
                var rect = listBox.GetItemRectangle(i);
                if (clientPoint.Y < rect.Top)
                    return Math.Max(0, i - 1);
            }

            return listBox.Items.Count - 1;
        }

        // 연속 인덱스 구간을 ListBox 선택 상태로 반영
        public static void SelectIndexRange(ListBox listBox, int anchor, int current)
        {
            if (listBox.Items.Count == 0) return;

            int lo = Math.Clamp(Math.Min(anchor, current), 0, listBox.Items.Count - 1);
            int hi = Math.Clamp(Math.Max(anchor, current), 0, listBox.Items.Count - 1);

            listBox.BeginUpdate();
            for (int i = 0; i < listBox.Items.Count; i++)
                listBox.SetSelected(i, i >= lo && i <= hi);
            listBox.EndUpdate();
        }

        public static bool IsIndexSelected(ListBox listBox, int index) =>
            index >= 0 && listBox.SelectedIndices.Cast<int>().Contains(index);

        public static bool IsPastDragThreshold(Point start, Point current)
        {
            var size = SystemInformation.DragSize;
            return Math.Abs(current.X - start.X) >= size.Width
                || Math.Abs(current.Y - start.Y) >= size.Height;
        }
    }
}
