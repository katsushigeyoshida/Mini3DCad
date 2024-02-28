using CoreLib;
using System.Windows;

namespace Mini3DCad
{
    /// <summary>
    /// SysPropertyDlg.xaml の相互作用ロジック
    /// </summary>
    public partial class SysPropertyDlg : Window
    {
        public double mArcDivideAngle = 30;
        public double mRevolutionDivideAngle = 30;
        public double mSweepDivideAngle = 30;
        public string mDataFolder = "";

        private string mDataFolderListPath = "DataFolderList.csv";
        private List<string> mDataFolderList = new List<string>();

        private YLib ylib = new YLib();

        public SysPropertyDlg()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbArcDivideAng.Text = ylib.double2StrZeroSup(ylib.R2D(mArcDivideAngle), "F8");
            tbRevolutionDivideAng.Text = ylib.double2StrZeroSup(ylib.R2D(mRevolutionDivideAngle), "F8");
            tbSweepDivideAng.Text = ylib.double2StrZeroSup(ylib.R2D(mSweepDivideAngle), "F8");
            List<string[]> llistf = ylib.loadCsvData(mDataFolderListPath);
            foreach (var buf in llistf) {
                if (!mDataFolderList.Contains(buf[0]))
                    mDataFolderList.Add(buf[0]);
            }
            cbDataFolder.Text = mDataFolder;
            if (0 < mDataFolder.Length)
                mDataFolderList.Insert(0, mDataFolder);
            cbDataFolder.ItemsSource = mDataFolderList;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            List<string[]> llist = new List<string[]>();
            foreach (var buf in mDataFolderList)
                llist.Add([buf]);
            ylib.saveCsvData(mDataFolderListPath, llist);
        }

        private void tbDataFolder_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string folder = ylib.folderSelect("データフォルダ", mDataFolder);
            if (folder != null && 0 < folder.Length)
                cbDataFolder.Text = folder;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mArcDivideAngle = ylib.D2R(ylib.string2double(tbArcDivideAng.Text));
            mRevolutionDivideAngle = ylib.D2R(ylib.string2double(tbRevolutionDivideAng.Text));
            mSweepDivideAngle = ylib.D2R(ylib.string2double(tbSweepDivideAng.Text));
            mDataFolder = cbDataFolder.Text;
            if (mDataFolderList.Contains(mDataFolder))
                mDataFolderList.Remove(mDataFolder);
            mDataFolderList.Insert(0, mDataFolder);

            DialogResult = true;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
