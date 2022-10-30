namespace MyCvModule
{
    public class UiTest : MyCvAlgorithmBase
    {
        private int intPara;
        public int IntPara
        {
            get => this.intPara;
            set
            {
                this.intPara = value;
                this.OnPropertyChanged();
            }
        }

        private double doublePara;
        public double DoublePara
        {
            get => this.doublePara;
            set
            {
                this.doublePara = value;
                this.OnPropertyChanged();
            }
        }

        private bool boolPara;
        public bool BoolPara
        {
            get => this.boolPara;
            set
            {
                this.boolPara = value;
                this.OnPropertyChanged();
            }
        }

        private System.IO.NotifyFilters enumPara;
        public System.IO.NotifyFilters EnumPara
        {
            get => this.enumPara;
            set
            {
                this.enumPara = value;
                this.OnPropertyChanged();
            }
        }

        public UiTest()
            : base()
        {
            this.intPara = 1;
            this.doublePara = 1.23;
            this.boolPara = false;
            this.enumPara = System.IO.NotifyFilters.FileName;
        }

        protected override void Calculate()
        {
            // Do nothing
        }
    }
}
