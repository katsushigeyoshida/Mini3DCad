using CoreLib;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Button = System.Windows.Controls.Button;
using Point = System.Windows.Point;

namespace Mini3DCad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double mWindowWidth;            //  ウィンドウの高さ
        private double mWindowHeight;           //  ウィンドウ幅
        private double mPrevWindowWidth;        //  変更前のウィンドウ幅
        private System.Windows.WindowState mWindowState = System.Windows.WindowState.Normal;  //  ウィンドウの状態(最大化/最小化)

        public string mAppName = "Mini3DCAD";                   //  アプリ名
        private string mHelpFile = "Mini3DCad_Manual.pdf";      //  マニュアルファイル
        public OPEMODE mOperationMode = OPEMODE.non;            //  操作モード(loc,pick)
        public OPEMODE mPrevOpeMode = OPEMODE.non;              //  操作モードの前回値
        private Point mPreMousePos;                             //  マウスの前回位置(screen座標)
        private PointD mPrePosition;                            //  マウスの前回位置(world座標)
        private bool mMouseLeftButtonDown = false;              //  左ボタン状態
        private bool mMouseRightButtonDown = false;             //  右ボタン状態
        private int mPickBoxSize = 10;                          //  ピック領域サイズ
        private int mMouseScroolSize = 5;                       //  マウスによるスクロール単位
        private double[] mGridSizeMenu = {                      //  グリッドサイズメニュー
            0, 0.1, 0.2, 0.25, 0.3, 0.4, 0.5, 1, 1.25, 1.5, 2, 2.5, 3, 4, 5, 10,
            20, 30, 40, 50, 100, 200, 300, 400, 500, 1000
        };

        public LocPick mLocPick;                            //  ロケイト・ピック処理
        public FileData mFileData;                          //  ファイル管理
        public DataDraw mDraw;                              //  描画クラス
        public DataManage mDataManage;                      //  データ管理クラス
        private CommandData mCommandData;                   //  コマンドデータ
        private CommandOpe mCommandOpe;                     //  コマンド処理
        private Canvas mCurCanvas;                          //  描画キャンバス
        private System.Windows.Controls.Image mCurImage;    //  描画イメージ

        public Color4 mBackColor = Color4.AliceBlue;        //  背景色
        private Vector3 mMin = new Vector3(-1, -1, -1);     //  表示領域の最小値
        private Vector3 mMax = new Vector3(1, 1, 1);        //  表示領域の最大値
        private double m3DScale = 5;                        //  3D表示の初期スケール

        public GL3DLib m3Dlib;                              //  三次元表示ライブラリ
        private GLControl glControl;                        //  OpenTK.GLcontrol
        private YLib ylib = new YLib();                     //  単なるライブラリ

        public MainWindow()
        {
            InitializeComponent();

            Title = mAppName;
            mAppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            //  OpenGL 初期化
            glControl = new GLControl();
            m3Dlib = new GL3DLib(glControl);
            m3Dlib.initPosition(1.3f, 0f, 0f, 0f);
            m3Dlib.setArea(mMin, mMax);
            //  OpenGLイベント処理
            glControl.Load       += glControl_Load;
            glControl.Paint      += glControl_Paint;
            glControl.Resize     += glControl_Resize;
            glControl.MouseDown  += glControl_MouseDown;
            glControl.MouseUp    += glControl_MouseUp;
            glControl.MouseMove  += glControl_MosueMove;
            glControl.MouseWheel += glControl_MouseWheel;
            glGraph.Child = glControl;                      //  OpenGLをWindowsFormsHostに接続

            mDataManage  = new DataManage(this);
            mFileData    = new FileData(this);
            mCommandData = new CommandData();
            mCommandOpe  = new CommandOpe(this, mDataManage);
            mLocPick     = new LocPick(mDataManage, this);

            WindowFormLoad();
        }

        /// <summary>
        /// 開始処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //  Canvas,Image,Faceの初期値設定
            mCurCanvas = cvCanvasFRONT;
            mCurImage = imScreenFRONT;
            mDataManage.mFace = FACE3D.FRONT;
            mDraw = new DataDraw(mCurCanvas, mCurImage, this);
            mDraw.mDataManage = mDataManage;
            mDraw.mLocPick = mLocPick;
            //  2D描画処理の初期化
            mDraw.drawWorldFrame();
            //  コントロールの初期化
            lbCommand.ItemsSource = mCommandData.getMainCommand();
            cbColor.DataContext = ylib.mBrushList;
            cbGridSize.ItemsSource = mGridSizeMenu;
            cbColor.SelectedIndex = ylib.getBrushNo(mDataManage.mPrimitiveBrush);
            cbGridSize.SelectedIndex = mGridSizeMenu.FindIndex(Math.Abs(mDraw.mGridSize));
            cbCommand.ItemsSource = mCommandOpe.mKeyCommand.mKeyCommandList;
            //  データファイルの設定
            mFileData.setBaseDataFolder();
            setDataFileList();
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mCommandOpe.mLayerChkListDlg != null)
                mCommandOpe.mLayerChkListDlg.Close();
            mCommandOpe.saveFile(true);
            mCommandOpe.saveKeycommnad();
            WindowFormSave();
        }

        /// <summary>
        /// Windowのサイズ変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_LayoutUpdated(object sender, EventArgs e)
        {
            if (WindowState != mWindowState && WindowState == System.Windows.WindowState.Maximized) {
                //  ウィンドウの最大化時
                mWindowWidth = SystemParameters.WorkArea.Width;
                mWindowHeight = SystemParameters.WorkArea.Height;
            } else if (WindowState != mWindowState ||
                mWindowWidth != Width || mWindowHeight != Height) {
                //  ウィンドウサイズが変わった時
                mWindowWidth = Width;
                mWindowHeight = Height;
            } else {
                //  ウィンドウサイズが変わらない時は何もしない
                mWindowState = WindowState;
                return;
            }
            mWindowState = WindowState;
            if (mDraw != null) {
                mDraw.drawWorldFrame();
                mDraw.draw();
            }
        }

        /// <summary>
        /// 状態の復元
        /// </summary>
        private void WindowFormLoad()
        {
            //  前回のWindowの位置とサイズを復元する(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.Reload();
            if (Properties.Settings.Default.MainWindowWidth < 100 ||
                Properties.Settings.Default.MainWindowHeight < 100 ||
                SystemParameters.WorkArea.Height < Properties.Settings.Default.MainWindowHeight) {
                Properties.Settings.Default.MainWindowWidth = mWindowWidth;
                Properties.Settings.Default.MainWindowHeight = mWindowHeight;
            } else {
                Top = Properties.Settings.Default.MainWindowTop;
                Left = Properties.Settings.Default.MainWindowLeft;
                Width = Properties.Settings.Default.MainWindowWidth;
                Height = Properties.Settings.Default.MainWindowHeight;
            }
            //  図面データ保存フォルダ
            if (0 < Properties.Settings.Default.BaseDataFolder.Length)
                mFileData.mBaseDataFolder = Properties.Settings.Default.BaseDataFolder;
            if (0 < Properties.Settings.Default.BackupFolder.Length)
                mFileData.mBackupFolder = Properties.Settings.Default.BackupFolder;
            mFileData.mDiffTool = Properties.Settings.Default.DiffTool;
            //  図面分類
            if (0 < Properties.Settings.Default.GenreName.Length)
                mFileData.mGenreName = Properties.Settings.Default.GenreName;
            if (0 < Properties.Settings.Default.CategoryName.Length)
                mFileData.mCategoryName = Properties.Settings.Default.CategoryName;
            if (0 < Properties.Settings.Default.DataName.Length)
                mFileData.mDataName = Properties.Settings.Default.DataName;
        }

        /// <summary>
        /// 状態の保存
        /// </summary>
        private void WindowFormSave()
        {
            //  図面分類
            Properties.Settings.Default.BaseDataFolder = mFileData.mBaseDataFolder;
            Properties.Settings.Default.BackupFolder = mFileData.mBackupFolder;
            Properties.Settings.Default.GenreName = mFileData.mGenreName;
            Properties.Settings.Default.CategoryName = mFileData.mCategoryName;
            Properties.Settings.Default.DataName = mFileData.mDataName;
            Properties.Settings.Default.DiffTool = mFileData.mDiffTool;

            //  Windowの位置とサイズを保存(登録項目をPropeties.settingsに登録して使用する)
            Properties.Settings.Default.MainWindowTop = Top;
            Properties.Settings.Default.MainWindowLeft = Left;
            Properties.Settings.Default.MainWindowWidth = Width;
            Properties.Settings.Default.MainWindowHeight = Height;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// OpenGLのLoad 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl_Load(object sender, EventArgs e)
        {
            m3Dlib.initLight();
        }

        /// <summary>
        /// OpenGLの描画 都度呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            renderFrame();
        }

        /// <summary>
        /// Windowのサイズが変わった時、glControl_Paintも呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl_Resize(object sender, EventArgs e)
        {
            GL.Viewport(glControl.ClientRectangle);
        }

        /// <summary>
        /// マウスホイールによるzoom up/down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            float delta = (float)e.Delta / 1000f;// - wheelPrevious;
            m3Dlib.setZoom(delta);
            renderFrame();
        }

        /// <summary>
        /// 視点(カメラ)の回転と移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl_MosueMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (m3Dlib.moveObject(e.X, e.Y))
                renderFrame();
        }

        /// <summary>
        /// マウスダウン 視点(カメラ)の回転開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) {
                m3Dlib.setMoveStart(true, e.X, e.Y);
            } else if (e.Button == MouseButtons.Right) {
                m3Dlib.setMoveStart(false, e.X, e.Y);
            }
        }

        /// <summary>
        /// マウスアップ 視点(カメラ)の回転終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            m3Dlib.setMoveEnd();
        }

        /// <summary>
        /// 三次元データ表示
        /// </summary>
        public void renderFrame()
        {
            m3Dlib.mWorldWidth = (int)glGraph.ActualWidth;
            m3Dlib.mWorldHeight = (int)glGraph.ActualHeight;
            if (m3Dlib.mWorldWidth == 0 || m3Dlib.mWorldHeight == 0)
                return;
            m3Dlib.setBackColor(mBackColor);
            m3Dlib.renderFrameStart();
            //  Surfaceデータの取得
            List<SurfaceData> slist = mDataManage.getSurfaceData();
            //  表示領域にはいるようにスケールと位置移動ベクトルを求める
            double scale = m3DScale / mDataManage.mArea.getSize();
            Point3D v = mDataManage.mArea.getCenter();
            v.inverse();
            //  データの登録
            for (int i = 0; i < slist.Count; i++) {
                m3Dlib.drawSurface(slist[i].mVertexList, slist[i].mDrawType, 
                    slist[i].mFaceColor, scale, v);
            }
            m3Dlib.setAreaFrameDisp(false);
            //m3Dlib.drawAxis(scale, v);
            m3Dlib.rendeFrameEnd();
        }

        /// <summary>
        /// マウスホィール(2D処理)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (0 != e.Delta) {
                //  2D表示
                double scaleStep = e.Delta > 0 ? 1.2 : 1 / 1.2;
                Point pos = e.GetPosition(mCurCanvas);
                PointD wp = mDraw.mGDraw.cnvScreen2World(new PointD(pos));
                mDraw.zoom(wp, scaleStep);
            }
        }

        /// <summary>
        /// マウスの移動(2D処理)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point pos = e.GetPosition(mCurCanvas);
            if (pos == mPreMousePos)
                return;
            PointD wpos = mDraw.mGDraw.cnvScreen2World(new PointD(pos));
            //  2D表示操作
            if ((mOperationMode == OPEMODE.areaDisp || mOperationMode == OPEMODE.areaPick) && !mDraw.mAreaLoc[0].isNaN()) {
                //  領域表示/選択
                mDraw.mAreaLoc[1] = wpos;
                mDraw.areaDragging(mDraw.mAreaLoc);
            } else if (mMouseLeftButtonDown && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                //  2Dスクロール
                if (mMouseScroolSize < ylib.distance(pos, mPreMousePos)) {
                    mDraw.scroll(pos.X - mPreMousePos.X, pos.Y - mPreMousePos.Y);
                } else
                    return;
            } else if (0 <= mLocPick.mLocList.Count) {
                //  ドラッギング表示
                mDraw.dragging(mCommandOpe.mOperation, mLocPick.mPickElement, mLocPick.mLocList, wpos);
            }
            dispStatus(wpos);
            mPreMousePos = pos;     //  スクリーン座標
            mPrePosition = wpos;    //  ワールド座標
        }

        /// <summary>
        /// マウス左ボタンダウン(2D処理)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mMouseLeftButtonDown = true;
            Point pos = e.GetPosition(mCurCanvas);
            PointD wpos = mDraw.mGDraw.cnvScreen2World(new PointD(pos));
            if (mOperationMode == OPEMODE.areaPick || mOperationMode == OPEMODE.areaDisp) {
                //  領域表示/ピック
                if (mDraw.areaOpe(wpos, mOperationMode))
                    mOperationMode = mPrevOpeMode;
            } else {
                if (0 < mDraw.mGridSize)
                    wpos.round(Math.Abs(mDraw.mGridSize));
            }
            if (mOperationMode == OPEMODE.loc) {
                //  ロケイトの追加
                if (mCommandOpe.mOperation == OPERATION.stretch && ylib.onAltKey())
                    mCommandOpe.mOperation = OPERATION.stretchArc;
                mLocPick.mLocList.Add(wpos);
            }
            //  データ登録(データ数固定コマンド)
            if (mDataManage.defineData(mCommandOpe.mOperation, mLocPick.mLocList, mLocPick.mPickElement))
                commandClear();
            dispStatus(wpos);
        }

        /// <summary>
        /// マウス左ボタンアップ(2D処理)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mMouseLeftButtonDown = false;
        }

        /// <summary>
        /// マウス右ボタンダウン(2D処理)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mMouseRightButtonDown = true;
            Point pos = e.GetPosition(mCurCanvas);
            if (mDataManage.mFace == FACE3D.NON)
                return;
            //  2D表示
            PointD wpos = mDraw.mGDraw.cnvScreen2World(new PointD(pos));
            List<int> picks = mLocPick.getPickNo(wpos, mDraw.mGDraw.screen2worldXlength(mPickBoxSize));
            if (mOperationMode == OPEMODE.loc) {
                mLocPick.autoLoc(wpos, picks);
                //  データ登録(データ数不定コマンド)
                if (mDataManage.defineData(mCommandOpe.mOperation, mLocPick.mLocList, mLocPick.mPickElement, 0 == picks.Count))
                    commandClear();
            } else {
                mLocPick.pickElement(wpos, picks, mOperationMode);  //  ピック要素登録
                mDraw.draw();
            }
            dispStatus(wpos);
        }

        /// <summary>
        /// マウス右ボタンアップ(2D処理)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mMouseRightButtonDown = false;
        }

        /// <summary>
        /// マウスのダブルクリック処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mOperationMode != OPEMODE.loc) {
                Point pos = e.GetPosition(mCurCanvas);
                PointD wpos = mDraw.mGDraw.cnvScreen2World(new PointD(pos));
                List<int> picks = mLocPick.getPickNo(wpos, mDraw.mGDraw.screen2worldXlength(mPickBoxSize));
                if (0 < picks.Count)
                    commandExec(OPERATION.changeProperty, mLocPick.mPickElement);
            }
        }

        /// <summary>
        /// タブ選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = (TabItem)tabCanvas.SelectedItem;
            if (item == null) return;
            if (item.Name == "CanvasFRONT") {
                mDataManage.mFace = FACE3D.FRONT;
                mCurCanvas = cvCanvasFRONT;
                mCurImage = imScreenFRONT;
            } else if (item.Name == "CanvasTOP") {
                mDataManage.mFace = FACE3D.TOP;
                mCurCanvas = cvCanvasTOP;
                mCurImage = imScreenTOP;
            } else if (item.Name == "CanvasRIGHT") {
                mDataManage.mFace = FACE3D.RIGHT;
                mCurCanvas = cvCanvasRIGHT;
                mCurImage = imScreenRIGHT;
            } else if (item.Name == "OpenGL") {
                mDataManage.mFace = FACE3D.NON;
            } else
                return;
            if (item.Name != "OpenGL" && mDraw != null) {
                mDraw.setCanvas(mCurCanvas, mCurImage, mDataManage.mFace);
                mDraw.draw2D();
            }
            if (mDraw != null)
                mDraw.mBitmapOn = false;    //  切り替え直後はBitmapが作られていない
            btDummy.Focus();                //  ダミーでフォーカスを外す
        }

        /// <summary>
        /// コマンド選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbCommand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = lbCommand.SelectedIndex;
            if (lbCommand.Items != null && 0 <= index) {
                string menu = lbCommand.Items[index].ToString() ?? "";
                COMMANDLEVEL level = mCommandData.getCommandLevl(menu);
                if (level == COMMANDLEVEL.main) {
                    //  メインコマンド
                    lbCommand.ItemsSource = mCommandData.getSubCommand(menu);
                } else if (level == COMMANDLEVEL.sub) {
                    //  サブコマンド
                    OPERATION ope = mCommandData.getCommand(menu);
                    commandExec(ope, mLocPick.mPickElement);
                }
                dispStatus(null);
            }
        }

        /// <summary>
        /// 色設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (0 <= cbColor.SelectedIndex)
                mDataManage.mPrimitiveBrush = ylib.mBrushList[cbColor.SelectedIndex].brush;
            btDummy.Focus();         //  ダミーでフォーカスを外す
        }

        /// <summary>
        /// グリッドの設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGridSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (0 <= cbGridSize.SelectedIndex) {
                mDraw.mGridSize = mGridSizeMenu[cbGridSize.SelectedIndex];
                mDraw.draw();
            }
            btDummy.Focus();         //  ダミーでフォーカスを外す
        }

        /// <summary>
        /// 大分類選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGenreList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbGenreList.SelectedIndex;
            if (0 <= index) {
                mFileData.setGenreFolder(cbGenreList.SelectedItem.ToString() ?? "");
                lbCategoryList.ItemsSource = mFileData.getCategoryList();
            }
        }

        /// <summary>
        /// ジャンル選択のコンテキストメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGenreMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            if (menuItem.Name.CompareTo("cbGenreAddMenu") == 0) {
                //  大分類(Genre)の追加
                string genre = mFileData.addGenre();
                if (0 < genre.Length) {
                    cbGenreList.ItemsSource = mFileData.getGenreList();
                    int index = cbGenreList.Items.IndexOf(genre);
                    if (0 <= index)
                        cbGenreList.SelectedIndex = index;
                }
            } else if (menuItem.Name.CompareTo("cbGenreRenameMenu") == 0) {
                //  大分類名の変更
                string genre = mFileData.renameGenre(cbGenreList.SelectedItem.ToString() ?? "");
                if (0 < genre.Length) {
                    cbGenreList.ItemsSource = mFileData.getGenreList();
                    int index = cbGenreList.Items.IndexOf(genre);
                    if (0 <= index)
                        cbGenreList.SelectedIndex = index;
                }
            } else if (menuItem.Name.CompareTo("cbGenreRemoveMenu") == 0) {
                //  大分類名の削除
                if (mFileData.removeGenre(cbGenreList.SelectedItem.ToString() ?? "")) {
                    cbGenreList.ItemsSource = mFileData.getGenreList();
                    if (0 < cbGenreList.Items.Count)
                        cbGenreList.SelectedIndex = 0;
                }
            }
            dispTitle();
        }

        /// <summary>
        /// 分類選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbCategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = lbCategoryList.SelectedIndex;
            if (0 <= index) {
                mFileData.setCategoryFolder(lbCategoryList.SelectedItem.ToString() ?? "");
                lbItemList.ItemsSource = mFileData.getItemFileList();
            }
        }

        /// <summary>
        /// カテゴリ選択のコンテキストメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbCategoryMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            string category = "";
            if (0 <= lbCategoryList.SelectedIndex)
                category = lbCategoryList.SelectedItem.ToString() ?? "";

            if (menuItem.Name.CompareTo("lbCategoryAddMenu") == 0) {
                //  分類(Category)の追加
                category = mFileData.addCategory();
                if (0 < category.Length) {
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    int index = lbCategoryList.Items.IndexOf(category);
                    if (0 <= index)
                        lbCategoryList.SelectedIndex = index;
                }
            } else if (menuItem.Name.CompareTo("lbCategoryRenameMenu") == 0) {
                //  分類名の変更
                category = mFileData.renameCategory(category);
                if (0 < category.Length) {
                    lbCategoryList.SelectedIndex = -1;
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    int index = lbCategoryList.Items.IndexOf(category);
                    if (0 <= index)
                        lbCategoryList.SelectedIndex = index;
                }
            } else if (menuItem.Name.CompareTo("lbCategoryRemoveMenu") == 0) {
                //  分類の削除
                if (mFileData.removeCategory(lbCategoryList.SelectedItem.ToString() ?? "")) {
                    lbCategoryList.SelectedIndex = -1;
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    if (0 < lbCategoryList.Items.Count)
                        lbCategoryList.SelectedIndex = 0;
                }
            } else if (menuItem.Name.CompareTo("lbCategoryCopyMenu") == 0) {
                //  分類のコピー
                mFileData.copyCategory(category);
            } else if (menuItem.Name.CompareTo("lbCategoryMoveMenu") == 0) {
                //  分類の移動
                if (mFileData.copyCategory(category, true)) {
                    lbCategoryList.SelectedIndex = -1;
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    if (0 < lbCategoryList.Items.Count)
                        lbCategoryList.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// 図面選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = lbItemList.SelectedIndex;
            if (0 <= index) {
                //  カレントデータ終了処理
                mCommandOpe.saveFile(true);
                if (mCommandOpe.mLayerChkListDlg != null)
                    mCommandOpe.mLayerChkListDlg.Close();
                //  新規データ読込
                mFileData.mDataName = lbItemList.Items[index].ToString() ?? "";
                mCommandOpe.mDataFilePath = mFileData.getCurItemFilePath();
                mCommandOpe.loadFile();
                //  パラメータ設定
                tabCanvas.SelectedIndex = -1;
                mDraw.mWorldList.Clear();
                cbColor.SelectedIndex = ylib.getBrushNo(mDataManage.mPrimitiveBrush);
                cbGridSize.SelectedIndex = mGridSizeMenu.FindIndex(Math.Abs(mDraw.mGridSize));
                tabCanvas.SelectedIndex = 0;
                commandClear(true);
                mDraw.mBitmapOn = false;
            }
            dispTitle();
        }

        /// <summary>
        /// 図面選択のコンテキストメニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbItemMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.Source;
            string itemName = null;
            if (0 <= lbItemList.SelectedIndex)
                itemName = lbItemList.SelectedItem.ToString() ?? "";

            mCommandOpe.saveFile(true);
            if (menuItem.Name.CompareTo("lbItemAddMenu") == 0) {
                //  図面(Item)の追加
                itemName = mFileData.addItem();
                if (0 < itemName.Length) {
                    mCommandOpe.newData(true);
                    commandClear();
                    mCommandOpe.mDataFilePath = mFileData.getItemFilePath(itemName);
                    mCommandOpe.saveFile(true);
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    lbItemList.SelectedIndex = lbItemList.Items.IndexOf(itemName);
                }
            } else if (menuItem.Name.CompareTo("lbItemRenameMenu") == 0 && itemName != null) {
                //  図面名の変更
                itemName = mFileData.renameItem(itemName);
                if (0 < itemName.Length) {
                    mCommandOpe.mDataFilePath = mFileData.getItemFilePath(itemName);
                    mCommandOpe.loadFile();
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    lbItemList.SelectedIndex = lbItemList.Items.IndexOf(itemName);
                }
            } else if (menuItem.Name.CompareTo("lbItemRemoveMenu") == 0 && itemName != null) {
                //  図面の削除
                if (mFileData.removeItem(itemName)) {
                    if (mCommandOpe.mDataFilePath == mFileData.getItemFilePath(itemName))
                        mDataManage.mElementList.Clear();
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    mCommandOpe.mDataFilePath = "";
                    if (0 < lbItemList.Items.Count)
                        lbItemList.SelectedIndex = 0;
                }
            } else if (menuItem.Name.CompareTo("lbItemCopyMenu") == 0 && itemName != null) {
                //  図面のコピー
                mFileData.copyItem(itemName);
            } else if (menuItem.Name.CompareTo("lbItemMoveMenu") == 0 && itemName != null) {
                //  図面の移動
                if (mFileData.copyItem(itemName, true)) {
                    mCommandOpe.mDataFilePath = "";
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    if (0 < lbItemList.Items.Count)
                        lbItemList.SelectedIndex = 0;
                }
            } else if (menuItem.Name.CompareTo("lbItemImportMenu") == 0 && itemName != null) {
                //  インポート
                string item = mFileData.importAsFile();
                mCommandOpe.mDataFilePath = "";
                lbItemList.ItemsSource = mFileData.getItemFileList();
                if (0 < lbItemList.Items.Count)
                    lbItemList.SelectedIndex = lbItemList.Items.IndexOf(item);
            } else if (menuItem.Name.CompareTo("lbItemPropertyMenu") == 0 && itemName != null) {
                //  図面のプロパティ
                string buf = mFileData.getItemFileProperty(itemName);
                ylib.messageBox(this, buf, "ファイルプロパティ");
            }
            dispTitle();
        }

        /// <summary>
        /// キー入力
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            keyCommand(e.Key, e.KeyboardDevice.Modifiers == ModifierKeys.Control, e.KeyboardDevice.Modifiers == ModifierKeys.Shift);
            //btDummy.Focus();         //  ダミーでフォーカスを外す
        }

        /// <summary>
        /// 2D画面の拡大縮小ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btZoom_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (button.Name.CompareTo("btZoomArea") == 0) {
                mPrevOpeMode = mOperationMode;
                mOperationMode = OPEMODE.areaDisp;
            } else if (button.Name.CompareTo("btZoomIn") == 0) {
                mDraw.zoom(mDraw.mGDraw.mWorld.getCenter(), 1.2);
            } else if (button.Name.CompareTo("btZoomOut") == 0) {
                mDraw.zoom(mDraw.mGDraw.mWorld.getCenter(), 1 / 1.2);
            } else if (button.Name.CompareTo("btZoomFit") == 0) {
                mDraw.dispFit();
            } else if (button.Name.CompareTo("btZoomWidthFit") == 0) {
            }
        }

        /// <summary>
        /// ロケイトメニューボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btMenu_Click(object sender, RoutedEventArgs e)
        {
            locMenu();
        }

        /// <summary>
        /// 領域ピックボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btAreaPick_Click(object sender, RoutedEventArgs e)
        {
            mPrevOpeMode = mOperationMode;
            mOperationMode = OPEMODE.areaPick;
        }

        /// <summary>
        /// キーコマンド入力コンボホックス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbCommand_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                if (mCommandOpe.keyCommand(cbCommand.Text, mDataManage.mFace)) {
                    cbCommand.ItemsSource = mCommandOpe.mKeyCommand.keyCommandList(cbCommand.Text);
                    commandClear();
                    dispTitle();
                }
                btDummy.Focus();         //  ダミーでフォーカスを外す
            }
        }

        /// <summary>
        /// システム設定ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSetting_Click(object sender, RoutedEventArgs e)
        {
            mDataManage.setSystemProperty();
        }

        /// <summary>
        /// [アンドゥ]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btUndo_Click(object sender, RoutedEventArgs e)
        {
            commandExec(OPERATION.undo, null);
        }

        /// <summary>
        /// [要素コピー]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btEntityCopy_Click(object sender, RoutedEventArgs e)
        {
            commandExec(OPERATION.copyElement, mLocPick.mPickElement);
        }

        /// <summary>
        /// [要素貼付]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btEntityPaste_Click(object sender, RoutedEventArgs e)
        {
            commandExec(OPERATION.pasteElement, null);
        }

        /// <summary>
        /// [画面コピー]ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btScreenCopy_Click(object sender, RoutedEventArgs e)
        {
            screenCopy();
        }

        /// <summary>
        /// ヘルプボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btHelp_Click(object sender, RoutedEventArgs e)
        {
            ylib.openUrl(mHelpFile);
        }

        /// <summary>
        /// コマンドの実行
        /// </summary>
        /// <param name="ope">コマンドコード</param>
        /// <param name="picks">ピックリスト</param>
        private void commandExec(OPERATION ope, List<PickData> picks)
        {
            mOperationMode = mCommandOpe.execCommand(ope, picks);
            if (mOperationMode == OPEMODE.clear || mOperationMode == OPEMODE.non)
                commandClear();
        }

        /// <summary>
        /// コマンド処理をクリア
        /// </summary>
        /// <param name="dispFit">全体表示</param>
        private void commandClear(bool dispFit = false)
        {
            mCommandOpe.mOperation = OPERATION.non;
            mOperationMode = OPEMODE.non;
            mLocPick.mLocList.Clear();
            mLocPick.mPickElement.Clear();
            lbCommand.ItemsSource = mCommandData.getMainCommand();
            lbCommand.SelectedIndex = -1;
            if (mDataManage.mFace == FACE3D.NON)
                renderFrame();
            if (mDataManage.mFace != FACE3D.NON && dispFit) {
                mDraw.dispFit();
            } else {
                mDraw.draw();
            }
        }

        /// <summary>
        /// 操作モードとマウス位置の表示
        /// </summary>
        /// <param name="wpos">マウス位置(World座標)</param>
        public void dispStatus(PointD wpos)
        {
            if (mPrePosition == null)
                return;
            if (wpos == null)
                wpos = mPrePosition;
            tbStatus.Text = $"Mode [{mOperationMode}] Pick [{mLocPick.mPickElement.Count}] Loc [{mLocPick.mLocList.Count}] Grid[{mDraw.mGridSize}] {wpos.ToString("f2")}";
        }

        /// <summary>
        /// 編集中の部品名の表示
        /// </summary>
        public void dispTitle()
        {
            string filename = Path.GetFileNameWithoutExtension(mCommandOpe.mDataFilePath);
            Title = $"{mAppName}[{filename}][{mDataManage.getElementCount()} / {mDataManage.mElementList.Count}]";
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="key"></param>
        /// <param name="control"></param>
        /// <param name="shift"></param>
        private void keyCommand(Key key, bool control, bool shift)
        {
            //  コマンド入力時は無効
            if (cbCommand.IsKeyboardFocusWithin)
                return;

            if (mDataManage.mFace == FACE3D.NON) {
                // 3D表示
                m3Dlib.keyMove(key, control, shift);
                renderFrame();
            } else {
                //  2D表示
                if (control) {
                    switch (key) {
                        case Key.S: mCommandOpe.execCommand(OPERATION.save, null); break;
                        case Key.Z: mCommandOpe.execCommand(OPERATION.undo, null); commandClear(); break;
                        default: mDraw.key2DMove(key, control, shift); break;
                    }
                } else {
                    switch (key) {
                        case Key.Escape: commandClear(); break;                                                 //  ESCキーでキャンセル
                        case Key.Back:                                      //  ロケイト点を一つ戻す
                            if (0 < mLocPick.mLocList.Count) {
                                mLocPick.mLocList.RemoveAt(mLocPick.mLocList.Count - 1);
                            }
                            break;
                        case Key.Apps: locMenu(); break;                    //  コンテキストメニューキー
                        default:
                            mDraw.key2DMove(key, control, shift);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 数値入力によるロケイトの選択メニュー
        /// </summary>
        private void locMenu()
        {
            mLocPick.locMenu(mCommandOpe.mOperation, mOperationMode);
            if (mDataManage.defineData(mCommandOpe.mOperation, mLocPick.mLocList, mLocPick.mPickElement))
                commandClear();
        }

        /// <summary>
        /// データファイルのリストをリストビューに設定する
        /// </summary>
        public void setDataFileList()
        {
            cbGenreList.ItemsSource = mFileData.getGenreList();
            int index = cbGenreList.Items.IndexOf(mFileData.mGenreName);
            if (0 <= index) {
                //  ジャンルを設定
                cbGenreList.SelectedIndex = index;
                index = lbCategoryList.Items.IndexOf(mFileData.mCategoryName);
                if (0 <= index) {
                    lbCategoryList.SelectedIndex = index;
                    index = lbItemList.Items.IndexOf(mFileData.mDataName);
                    if (0 <= index) {
                        lbItemList.SelectedIndex = index;
                    }
                }
            } else {
                //  ジャンル不定
                if (0 < cbGenreList.Items.Count) {
                    mFileData.mGenreName = cbGenreList.Items[0].ToString() ?? "";
                    lbCategoryList.ItemsSource = mFileData.getCategoryList();
                    if (0 < lbCategoryList.Items.Count) {
                        mFileData.mCategoryName = lbCategoryList.Items[0].ToString() ?? "";
                        lbItemList.ItemsSource = mFileData.getItemFileList();
                    }
                }
            }
        }

        /// <summary>
        /// データファイルのリストを読み直しする
        /// </summary>
        public void reloadDataFileList()
        {
            cbGenreList.SelectedIndex = -1;
            cbGenreList.ItemsSource = mFileData.getGenreList();
            if (0 < cbGenreList.Items.Count) {
                mFileData.mGenreName = cbGenreList.Items[0].ToString() ?? "";
                lbCategoryList.ItemsSource = mFileData.getCategoryList();
                if (0 < lbCategoryList.Items.Count) {
                    mFileData.mCategoryName = lbCategoryList.Items[0].ToString() ?? "";
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                }
                if (0 < lbCategoryList.Items.Count)
                    cbGenreList.SelectedIndex = 0;
                if (0 < lbItemList.Items.Count)
                    lbItemList.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 作図領域の画面コピー
        /// </summary>
        public void screenCopy()
        {
            if (mDataManage.mFace == FACE3D.NON) {
                m3Dlib.screenCopy();
            } else {
                mDraw.screenCopy();
            }
        }

        /// <summary>
        /// 作図領域の画面をファイル保存
        /// </summary>
        public void screenSave()
        {
            BitmapSource bitmapSource;
            if (mDataManage.mFace == FACE3D.NON) {
                bitmapSource = ylib.bitmap2BitmapSource(m3Dlib.ToBitmap());
            } else {
                bitmapSource = mDraw.toBitmapScreen();
            }
            if (bitmapSource != null) {
                string path = ylib.fileSaveSelectDlg("イメージ保存", ".", mDataManage.mImageFilters);
                if (0 < path.Length) {
                    if (Path.GetExtension(path).Length == 0)
                        path += ".png";
                    ylib.saveBitmapImage(bitmapSource, path);
                }
            }
        }
    }
}