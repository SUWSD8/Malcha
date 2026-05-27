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
            Form2 trainForm = new Form2();
            trainForm.StartPosition = FormStartPosition.Manual;
            trainForm.Location = this.Location;
            trainForm.Size = this.Size;

            // 핵심: Form2가 닫힐 때 프로그램 전체를 끄는 게 아니라, 숨겨져 있던 Form1을 다시 보여줍니다.
            trainForm.FormClosed += (s, args) => {
                if (Application.OpenForms["Form1"] is Form1 mainForm)
                {
                    mainForm.Location = trainForm.Location; // Form2의 위치를 다시 Form1에 동기화
                    mainForm.Show();
                }
            };

            trainForm.Show();
            this.Hide();
        }
    }
}
//