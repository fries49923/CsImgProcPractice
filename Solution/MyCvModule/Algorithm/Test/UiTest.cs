using CommunityToolkit.Mvvm.ComponentModel;

namespace MyCvModule
{
    public partial class UiTest : MyCvAlgorithmBase
    {
        [ObservableProperty]
        private int _intPara;

        [ObservableProperty]
        private double _doublePara;

        [ObservableProperty]
        private bool _boolPara;

        [ObservableProperty]
        private System.IO.NotifyFilters _enumPara;

        public UiTest()
            : base()
        {
            _intPara = 1;
            _doublePara = 1.23;
            _boolPara = false;
            _enumPara = System.IO.NotifyFilters.FileName;
        }

        protected override void Calculate()
        {
            // Do nothing
        }
    }
}
