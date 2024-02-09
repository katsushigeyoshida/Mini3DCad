using CoreLib;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private string mAppName = "Mini3DCAD";
        private int mPickBoxSize = 10;                          //  ピック領域サイズ
        private int mMouseScroolSize = 5;                       //  マウスによるスクロール単位
        public OPEMODE mOperationMode = OPEMODE.non;            //  操作モード(loc,pick)
        public OPEMODE mPrevOpeMode = OPEMODE.non;              //  操作モードの前回値
        private Point mPreMousePos;                             //  マウスの前回位置(screen座標)
        private PointD mPrePosition;                            //  マウスの前回位置(world座標)
        private bool mMouseLeftButtonDown = false;              //  左ボタン状態
        private bool mMouseRightButtonDown = false;             //  右ボタン状態
        private double[] mGridSizeMenu = {
            0, 0.1, 0.2, 0.25, 0.3, 0.4, 0.5, 1, 1.25, 1.5, 2, 2.5, 3, 4, 5, 10,
            20, 30, 40, 50, 100, 200, 300, 400, 500, 1000
        };

        public DataDraw mDraw;                              //  描画クラス
        public DataManage mDataManage;                      //  データ管理クラス
        private CommandData mCommandData;                   //  コマンドデータ
        private CommandOpe mCommandOpe;                     //  コマンド処理
        private FileData mFileData;                         //  ファイル管理
        private Canvas mCurCanvas;                          //  描画キャンバス
        private System.Windows.Controls.Image mCurImage;    //  描画イメージ

        public Color4 mBackColor = Color4.AliceBlue;        //  背景色
        private Vector3 mMin = new Vector3(-1, -1, -1);     //  表示領域の最小値
        private Vector3 mMax = new Vector3(1, 1, 1);        //  表示領域の最大値
        private double m3DScale = 5;                        //  3D表示の初期スケール

        private GLControl glControl;                        //  OpenTK.GLcontrol
        private GL3DLib m3Dlib;                             //  三次元表示ライブラリ
        private YLib ylib = new YLib();                     //  単なるライブラリ

        public MainWindow()
        {
            InitializeComponent();

            Title = mAppName;

            //  OpenGL 初期化
            glControl = new GLControl();
            m3Dlib = new GL3DLib(glControl);
            m3Dlib.initPosition(1.3f, -70f, 0f, 10f);
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
            mDataManage.mFace = FACE3D.XY;
            mDraw = new DataDraw(mCurCanvas, mCurImage, this);
            mDraw.mDataManage = mDataManage;
            //  2D描画処理の初期化
            mDraw.drawWorldFrame();
            mDraw.draw();
            //  コントロールの初期化
            lbCommand.ItemsSource = mCommandData.getMainCommand();
            cbColor.DataContext = ylib.mBrushList;
            cbGridSize.ItemsSource = mGridSizeMenu;
            cbColor.SelectedIndex = ylib.getBrushNo(mDataManage.mPrimitiveBrush);
            cbGridSize.SelectedIndex = mGridSizeMenu.FindIndex(Math.Abs(mDraw.mGridSize));
            //  データファイルの設定
            mFileData.setBaseDataFolder();
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
        /// 終了処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mCommandOpe.saveFile();
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
            System.Diagnostics.Debug.WriteLine("Window_LayoutUpdated");
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
            Properties.Settings.Default.GenreName = mFileData.mGenreName;
            Properties.Settings.Default.CategoryName = mFileData.mCategoryName;
            Properties.Settings.Default.DataName = mFileData.mDataName;

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
            GL.Enable(EnableCap.DepthTest);         //  デプスバッファ
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.Lighting);          //  光源の使用
            GL.PointSize(3.0f);                     //  点の大きさ
            GL.LineWidth(1.5f);                     //  線の太さ

            //m3Dlib.setLight();
            //m3Dlib.setMaterial();
            float[] position0 = new float[] { 2.0f, 2.0f, 2.0f, 0.0f };
            GL.Light(LightName.Light0, LightParameter.Position, position0);
            GL.Enable(EnableCap.Light0);
            float[] position1 = new float[] { -2.0f, -2.0f, 2.0f, 0.0f };
            GL.Light(LightName.Light1, LightParameter.Position, position1);
            GL.Enable(EnableCap.Light1);
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
        private void renderFrame()
        {
            //if (mPositionList == null)
            //    return;
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
            m3Dlib.drawAxis(scale, v);
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
            } else
            if (0 < mDataManage.mLocList.Count) {
                //  ドラッギング表示
                mDraw.dragging(mCommandOpe.mOperation, mDataManage.mPickElement, mDataManage.mLocList, wpos);
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
            }
            if (0 < mDraw.mGridSize)
                wpos.round(Math.Abs(mDraw.mGridSize));
            if (mOperationMode == OPEMODE.loc) {
                //  ロケイトの追加
                mDataManage.mLocList.Add(wpos);
            }
            //  データ登録(データ数固定コマンド)
            if (mDataManage.defineData(mCommandOpe.mOperation))
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
            List<int> picks = mDataManage.getPickNo(wpos, mDraw.mGDraw.screen2worldXlength(mPickBoxSize));
            if (mOperationMode == OPEMODE.loc) {
                mDataManage.autoLoc(wpos, picks);
                //  データ登録(データ数不定コマンド)
                if (mDataManage.defineData(mCommandOpe.mOperation, true))
                    commandClear();
            } else {
                mDataManage.pickElement(wpos, picks, mOperationMode);
                //  ピック色表示
                mDataManage.setPick();
                mDraw.draw();
                mDataManage.pickReset();
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
        /// タブ選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = (TabItem)tabCanvas.SelectedItem;
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
                    mOperationMode = mCommandOpe.execCommand(ope, mDataManage.mPickElement);
                    if (mOperationMode == OPEMODE.clear || mOperationMode == OPEMODE.non) {
                        commandClear();
                    }
                }
                dispStatus(null);
            }
        }

        /// <summary>
        /// コマンド処理をクリア
        /// </summary>
        /// <param name="dispFit"></param>
        private void commandClear(bool dispFit = false)
        {
            mCommandOpe.mOperation = OPERATION.non;
            mOperationMode = OPEMODE.non;
            mDataManage.mLocList.Clear();
            mDataManage.mPickElement.Clear();
            lbCommand.ItemsSource = mCommandData.getMainCommand();
            lbCommand.SelectedIndex = -1;
            if (mDataManage.mFace != FACE3D.NON && dispFit) {
                mDraw.dispFit();
            } else {
                mDraw.draw();
            }
        }

        /// <summary>
        /// 操作モードとマウス位置の表示
        /// </summary>
        /// <param name="wpos"></param>
        private void dispStatus(PointD wpos)
        {
            if (mPrePosition == null)
                return;
            if (wpos == null)
                wpos = mPrePosition;
            tbStatus.Text = $"Mode [{mOperationMode}] Pick [{mDataManage.mPickElement.Count}] Loc [{mDataManage.mLocList.Count}] Grid[{mDraw.mGridSize}] {wpos.ToString("f2")}";
        }

        /// <summary>
        /// 編集中の部品名の表示
        /// </summary>
        private void dispTitle()
        {
            string filename = Path.GetFileNameWithoutExtension(mCommandOpe.mDataFilePath);
            Title = $"{mAppName}[{filename}][{mDataManage.mElementList.Count}]";
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
                mCommandOpe.saveFile(true);
                mFileData.mDataName = lbItemList.Items[index].ToString() ?? "";
                mCommandOpe.mDataFilePath = mFileData.getCurItemFilePath();
                mCommandOpe.loadFile();
                cbColor.SelectedIndex = ylib.getBrushNo(mDataManage.mPrimitiveBrush);
                cbGridSize.SelectedIndex = mGridSizeMenu.FindIndex(Math.Abs(mDraw.mGridSize));
                //mDataManage.mFace = (FACE3D)Enum.ToObject(typeof(FACE3D), tabCanvas.SelectedIndex);
                mDataManage.mFace = FACE3D.XY;
                tabCanvas.SelectedIndex = (int)mDataManage.mFace;
                commandClear(true);
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
                    lbItemList.ItemsSource = mFileData.getItemFileList();
                    mCommandOpe.mDataFilePath = "";
                    if (0 < lbItemList.Items.Count)
                        lbItemList.SelectedIndex = 0;
                }
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
        /// キー入力処理
        /// </summary>
        /// <param name="key"></param>
        /// <param name="control"></param>
        /// <param name="shift"></param>
        private void keyCommand(Key key, bool control, bool shift)
        {
            if (mDraw.mFace == FACE3D.NON) {
                // 3D表示
            } else {
                //  2D表示
                switch (key) {
                    case Key.F2: mPrevOpeMode = mOperationMode; mOperationMode = OPEMODE.areaDisp; break;
                    case Key.F7: mPrevOpeMode = mOperationMode; mOperationMode = OPEMODE.areaPick; break;
                    default:
                        mDraw.key2DMove(key, control, shift);
                        break;
                }
            }
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

        private void cbCommand_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                if (mCommandOpe.keyCommand(cbCommand.Text)) {
                    //    keyCommandList(cbCommand.Text);
                    //    disp(mEntityData);
                }
            }
        }
    }
}