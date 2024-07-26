using CoreLib;
using System.Windows;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Mini3DCad
{
    /// <summary>
    /// PropertyDlg.xaml の相互作用ロジック
    /// </summary>
    public partial class PropertyDlg : Window
    {
        public string mName = "";                   //  要素名
        public bool mNameEnable = false;
        public Brush mLineColor = Brushes.Black;    //  2D表示色
        public string mLineColorName = "Black";
        public bool mLineColoeEnable = false;
        public Brush mFaceColor = Brushes.Blue;     //  3D表示色
        public string mFaceColorName = "Blue";
        public bool mFaceColorEnable = false;
        public int mLineFont = 0;                   //  線種
        public bool mLineFontOn = true;
        public bool mLineFontEnable = false;
        public bool mBothShading = false;           //  両面表示
        public bool mBothShadingEnable = false;
        public bool mDisp2D = true;                 //  2D表示
        public bool mDisp2DEnable = false;
        public bool mDisp3D = true;                 //  3D表示
        public bool mDisp3DEnable = false;
        public bool mEdgeDisp = true;               //  端面表示
        public bool mEdgeDispEnable = false;
        public bool mOutlineDisp = false;           //  外枠表示
        public bool mOutlineDispEnable = false;
        public double mArcRadius = 1;               //  円弧半径(円)
        public bool mArcRadiusEnable = false;
        public bool mArcRadiusOn = true;
        public double mArcStartAngle = 0;           //  円弧開始角(円弧、回転体、スィープ)
        public bool mArcStartAngleEnable = false;
        public double mArcEndAngle = Math.PI * 2;   //  円弧終了角(円弧、回転体、スィープ
        public bool mArcOn = false;
        public bool mArcEndAngleEnable = false;
        public bool mReverseOn = false;             //  逆順(ポリライン、ポリゴン、押出、ブレンド)
        public bool mReverse = false;
        public bool mReverseEnable = false;
        public bool mDivideAngOn = false;           //  円弧分割角度
        public double mDivideAng = 15;
        public bool mDivideAngEnable = false;
        public bool mPropertyAll = false;           //  複数要素設定
        public List<CheckBoxListItem> mChkList;     //  レイヤー使用リスト
        public bool mCkkListAdd = true;             //  レイヤ追加チェック
        public bool mCkkListEnable = false;
        public string mGroup = "";                  //  グループ名
        public List<string> mGroupList;
        public bool mGroupEnable = false;

        private string[] mLineFontName = new string[] {
            "実線", "破線", "一点鎖線", "二点鎖線"};
        private YLib ylib = new YLib();

        public PropertyDlg()
        {
            InitializeComponent();

            //lbNameTitle.Visibility = Visibility.Hidden;
            //tbName.Visibility = Visibility.Hidden;
            cbLineColor.DataContext = ylib.mBrushList;
            cbFaceColor.DataContext = ylib.mBrushList;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbName.Text = mName;
            int colorindex = ylib.getBrushNo(mLineColor);
            if (0 <= colorindex)
                cbLineColor.SelectedIndex = colorindex;
            colorindex = ylib.getBrushNo(mFaceColor);
            if (0 <= colorindex)
                cbFaceColor.SelectedIndex = ylib.getBrushNo(mFaceColor);
            cbLineFont.ItemsSource   = mLineFontName;
            cbLineFont.SelectedIndex = mLineFont;
            cbLineFont.IsEnabled     = mLineFontOn;
            //chBothShading.IsChecked  = mBothShading;
            //chBothShading.IsEnabled  = mBothShadingEnable;
            chDisp2D.IsChecked       = mDisp2D;
            chDisp3D.IsChecked       = mDisp3D;
            chEdgeDisp.IsChecked     = mEdgeDisp;
            chEdgeDisp.IsEnabled     = mEdgeDispEnable;
            chOutlineDisp.IsChecked  = mOutlineDisp;
            chOutlineDisp.IsEnabled  = mOutlineDispEnable;
            tbArcRadius.Text         = mArcRadius.ToString();
            tbArcStartAngle.Text     = ylib.double2StrZeroSup(mArcStartAngle);
            tbArcEndAngle.Text       = ylib.double2StrZeroSup(mArcEndAngle);
            //lbReverseTitle.Visibility = mReverseOn ? Visibility.Visible : Visibility.Collapsed;
            //chReverse.Visibility = mReverseOn ? Visibility.Visible : Visibility.Collapsed;
            chReverse.IsEnabled = mReverseOn ? true : false;
            chReverse.IsChecked = mReverse;
            //lbDivideAngTitle.Visibility = mDivideAngOn ? Visibility.Visible : Visibility.Collapsed;
            //tbDivideAng.Visibility = mDivideAngOn ? Visibility.Visible : Visibility.Collapsed;
            tbDivideAng.IsEnabled = mDivideAngOn ? true : false;
            tbDivideAng.Text      = ylib.double2StrZeroSup(mDivideAng);
            //cbLayerList.ItemsSource = mChkList;
            cbLayerList.Items.Clear();
            mChkList.ForEach(p => cbLayerList.Items.Add(p));
            chLayerListAdd.IsChecked = mCkkListAdd;
            cbGroup.ItemsSource = mGroupList;
            cbGroup.Text = mGroup;

            if (!mArcOn) {
                tbArcRadius.IsEnabled = false;
                tbArcStartAngle.IsEnabled = false;
                tbArcEndAngle.IsEnabled = false;
            }
            if (!mArcRadiusOn)
                tbArcRadius.IsEnabled = false;


            if (!mPropertyAll) {
                //  一括変更以外
                chNameEnable.Visibility          = Visibility.Hidden;
                chLineColorEnable.Visibility     = Visibility.Hidden;
                chFaceColorEnable.Visibility     = Visibility.Hidden;
                chLineFontEnable.Visibility      = Visibility.Hidden;
                //chBothShadingEnable.Visibility   = Visibility.Hidden;
                chDisp2DEnable.Visibility        = Visibility.Hidden;
                chDisp3DEnable.Visibility        = Visibility.Hidden;
                chEdgeDispEnable.Visibility      = Visibility.Hidden;
                chOutlineDispEnable.Visibility   = Visibility.Hidden;
                chArcRadiusEnable.Visibility     = Visibility.Hidden;
                chArcStartAngleEnable.Visibility = Visibility.Hidden;
                chArcEndAngleEnable.Visibility   = Visibility.Hidden;
                chReverseEnable.Visibility       = Visibility.Hidden;
                chDivideAngEnable.Visibility     = Visibility.Hidden;
                chLayerListAdd.Visibility        = Visibility.Hidden;
                chLayerListEnable.Visibility     = Visibility.Hidden;
                chGroupEnable.Visibility         = Visibility.Hidden;
            }
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        /// <summary>
        /// OKボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            mName = tbName.Text;
            if (0 <= cbLineColor.SelectedIndex) {
                mLineColor = ylib.mBrushList[cbLineColor.SelectedIndex].brush;
                mLineColorName = ylib.mBrushList[cbLineColor.SelectedIndex].colorTitle;
            }
            mNameEnable = chNameEnable.IsChecked == true;
            mLineFont = cbLineFont.SelectedIndex;
            mLineFontEnable = chLineFontEnable.IsChecked == true;
            if (0 <= cbFaceColor.SelectedIndex) {
                mFaceColor = ylib.mBrushList[cbFaceColor.SelectedIndex].brush;
                mFaceColorName = ylib.mBrushList[cbFaceColor.SelectedIndex].colorTitle;
            }
            mLineColoeEnable   = chLineColorEnable.IsChecked == true;
            mFaceColorEnable   = chFaceColorEnable.IsChecked == true;
            //mBothShading       = chBothShading.IsChecked == true;
            //mBothShadingEnable = chBothShadingEnable.IsChecked == true;
            mDisp2D            = chDisp2D.IsChecked == true;
            mDisp2DEnable      = chDisp2DEnable.IsChecked == true;
            mDisp3D = chDisp3D.IsChecked == true;
            mDisp3DEnable = chDisp3DEnable.IsChecked == true;
            mEdgeDisp = chEdgeDisp.IsChecked == true;
            mEdgeDispEnable    = chEdgeDispEnable.IsChecked == true;
            mOutlineDisp       = chOutlineDisp.IsChecked == true;
            mOutlineDispEnable = chOutlineDispEnable.IsChecked == true;
            mArcRadius         = ylib.doubleParse(tbArcRadius.Text, 1);
            mArcStartAngle     = ylib.doubleParse(tbArcStartAngle.Text, 1);
            mArcEndAngle       = ylib.doubleParse(tbArcEndAngle.Text, 1);
            mReverse           = chReverse.IsChecked == true;
            mReverseEnable     = chReverseEnable.IsChecked == true;
            mDivideAng         = ylib.doubleParse(tbDivideAng.Text, 10);
            mDivideAngEnable   = chDivideAngEnable.IsChecked == true;
            mCkkListAdd        = chLayerListAdd.IsChecked == true;
            mCkkListEnable     = chLayerListEnable.IsChecked == true;
            mGroup             = cbGroup.Text;
            mGroupEnable       = chGroupEnable.IsChecked == true;

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// キャンセルボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// レイヤ名の追加(レイヤ名のダブルクリック)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbLayerList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            InputBox dlg = new InputBox();
            dlg.Name = "レイヤー名の追加";
            dlg.Owner = this;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (dlg.ShowDialog() == true) {
                CheckBoxListItem item = new CheckBoxListItem(false, dlg.mEditText);
                if (0 > mChkList.FindIndex(p => p.Text.CompareTo(item.Text) == 0)) {
                    mChkList.Add(item);
                    cbLayerList.Items.Clear();
                    mChkList.ForEach(p => cbLayerList.Items.Add(p));
                }
            }
        }
    }
}
