namespace Malcha
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        
        private void btnTrainModel_Click(object sender, EventArgs e)
        {
            // 1. 새로 만든 모델 학습 폼(FormTrain)의 인스턴스를 생성합니다.
            Form2 trainForm = new Form2(); // 좌측 타입을 Form2로 정확하게 맞춰줍니다!

            // 2. 새 창이 메인 창(Form1)의 정중앙에 위치하도록 띄우는 설정입니다. (UI가 깔끔해짐)
            trainForm.StartPosition = FormStartPosition.CenterParent;

            // 3. 핵심! ShowDialog()를 사용하여 '모달' 방식으로 창을 열어줍니다.
            // (this)를 넣어주면 메인 폼이 새 폼의 부모(Owner)가 되어 관계가 명확해집니다.
            trainForm.ShowDialog(this);
        }
    }
}
//