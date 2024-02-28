using CoreLib;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Mini3DCad
{
    /// <summary>
    /// SelectCategory.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectCategory : Window
    {
        public double mWindowWidth;                         //  ウィンドウの高さ
        public double mWindowHeight;                        //  ウィンドウ幅
        public bool mWindowSizeOutSet = false;              //  ウィンドウサイズの外部設定

        public string mRootFolder;                          //  データのルートディレクトリ
        public List<string> mGenreList;                     //  Genreリスト
        public List<string> mCategoryList;                  //  Categoryリスト
        public string mSelectGenre = "";                    //  選択genre
        public string mSelectCategory = "";                 //  選択Category

        private YLib ylib = new YLib();

        public SelectCategory()
        {
            InitializeComponent();

            mWindowWidth = Width;
            mWindowHeight = Height;
            WindowFormLoad();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            getGenreList();
            getCategoryList();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowFormSave();
        }

        private void cbGenre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getCategoryList();
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            if (0 <= cbGenre.SelectedIndex)
                mSelectGenre = cbGenre.Items[cbGenre.SelectedIndex].ToString();
            if (0 <= lbCategory.SelectedIndex)
                mSelectCategory = lbCategory.Items[lbCategory.SelectedIndex].ToString();

            DialogResult = true;
            Close();
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Windowの状態を前回の状態にする
        /// </summary>
        private void WindowFormLoad()
        {
            //  前回のWindowの位置とサイズを復元する(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.SelectCategoryWidth < 100 ||
                Properties.Settings.Default.SelectCategoryHeight < 100 ||
                SystemParameters.WorkArea.Height < Properties.Settings.Default.SelectCategoryHeight) {
                Properties.Settings.Default.SelectCategoryWidth = mWindowWidth;
                Properties.Settings.Default.SelectCategoryHeight = mWindowHeight;
            } else {
                Top = Properties.Settings.Default.SelectCategoryTop;
                Left = Properties.Settings.Default.SelectCategoryLeft;
                Width = Properties.Settings.Default.SelectCategoryWidth;
                Height = Properties.Settings.Default.SelectCategoryHeight;
            }
        }

        /// <summary>
        /// Window状態を保存する
        /// </summary>
        private void WindowFormSave()
        {
            //  Windowの位置とサイズを保存(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.SelectCategoryTop = Top;
            Properties.Settings.Default.SelectCategoryLeft = Left;
            Properties.Settings.Default.SelectCategoryWidth = Width;
            Properties.Settings.Default.SelectCategoryHeight = Height;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Genre(大分類ディレクトリ)リストの取得
        /// </summary>
        private void getGenreList()
        {
            mGenreList = ylib.getDirectories(mRootFolder);
            cbGenre.ItemsSource = mGenreList.ConvertAll(x => Path.GetFileName(x));
            if (0 < cbGenre.Items.Count)
                cbGenre.SelectedIndex = 0;
        }

        /// <summary>
        /// Category(小分類ディレクトリ)リストの取得
        /// </summary>
        private void getCategoryList()
        {
            int genreIndex = cbGenre.SelectedIndex < 0 ? 0 : cbGenre.SelectedIndex;
            string genre = cbGenre.Items[genreIndex].ToString();
            string categroryPath = Path.Combine(mRootFolder, genre);
            mCategoryList = ylib.getDirectories(categroryPath);
            lbCategory.ItemsSource = mCategoryList.ConvertAll(x => Path.GetFileName(x));
        }
    }
}
