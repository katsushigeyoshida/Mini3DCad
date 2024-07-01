using CoreLib;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Mini3DCad
{
    public class DataDraw
    {
        public double mWorldSize = 2.0;                         //  3D 空間サイズ
        public double mGridSize = 1.0;                          //  グリッドサイズ
        public Brush mBaseBackColor = Brushes.White;            //  2D背景色
        public BitmapSource mBitmapSource;                      //  CanvasのBitmap一時保存
        public Brush mPickColor = Brushes.Red;                  //  ピック時のカラー
        public int mScrollSize = 19;                            //  キーによるスクロール単位
        public int mGridMinmumSize = 8;                         //  グリッドの最小スクリーンサイズ
        public FACE3D mFace = FACE3D.XY;                        //  表示モード(XY,YZ,ZX,3D)
        public bool mBitmapOn = false;                          //  Bitmap取得状態
        public List<PointD> mAreaLoc = new List<PointD>() {     //  領域座標
            new PointD(), new PointD()
        };
        public Dictionary<FACE3D, Box> mWorldList = new Dictionary<FACE3D, Box>();  //  タブごとの表示領域

        public LocPick mLocPick;
        public DataManage mDataManage;
        public YWorldDraw mGDraw;                                  //  2D/3D表示ライブラリ

        private MainWindow mMainWindow;
        private System.Windows.Controls.Image mImScreen;
        private Canvas mCanvas;
        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="canvas">Canvas</param>
        /// <param name="imScreen">Image</param>
        /// <param name="mainWindow">MainWindow</param>
        public DataDraw(Canvas canvas, System.Windows.Controls.Image imScreen, MainWindow mainWindow)
        {
            mCanvas = canvas;
            mImScreen = imScreen;
            mMainWindow = mainWindow;
            mWorldSize = 30.0;
            mGDraw = new YWorldDraw(mCanvas, mCanvas.ActualWidth, mCanvas.ActualHeight);
            set3DWorldWindow(new System.Windows.Size(mCanvas.ActualWidth, mCanvas.ActualHeight), mWorldSize);
            mGDraw.clear();
            mGDraw.mClipping = true;
        }

        /// <summary>
        /// CanvasとImageを再設定
        /// </summary>
        /// <param name="canvas">Canvas</param>
        /// <param name="imScreen">Image</param>
        /// <param name="face">2D平面</param>
        public void setCanvas(Canvas canvas, System.Windows.Controls.Image imScreen, FACE3D face)
        {
            if (!mWorldList.ContainsKey(mFace)) {
                mWorldList.Add(mFace, mGDraw.mWorld.toCopy());
            } else {
                mWorldList[mFace] = mGDraw.mWorld.toCopy();
            }
            mCanvas = canvas;
            mImScreen = imScreen;
            mFace = face;
            double width = mCanvas.ActualWidth;
            double height = mCanvas.ActualHeight;
            if (mCanvas.ActualWidth == 0 || mCanvas.ActualHeight == 0) {
                width = mGDraw.mView.Width;
                height = mGDraw.mView.Height;
            }
            if (!mWorldList.ContainsKey(mFace)) {
                mWorldList.Add(mFace, mDataManage.mArea.toBox(mFace));
            }
            mGDraw = new YWorldDraw(mCanvas, width, height);
            mGDraw.setWorldWindow(mWorldList[mFace]);
            mGDraw.clear();
            mGDraw.mClipping = true;
        }

        /// <summary>
        /// ドラッギング表示
        /// </summary>
        /// <param name="ope">操作コード</param>
        /// <param name="pickData">ピックデータ</param>
        /// <param name="locList">ロケイトデータ</param>
        /// <param name="lastPoint">最終ロケイト点</param>
        public void dragging(OPERATION ope, List<PickData> pickData, List<PointD> locList, PointD lastPoint)
        {
            if (ope == OPERATION.non)
                return;
            mGDraw.mBrush = Brushes.Green;
            mGDraw.mFillColor = null;
            mGDraw.mLineType = 0;
            mGDraw.mThickness = 1;

            mGDraw.clear();
            if (mImScreen != null && mBitmapSource != null && mBitmapOn) {
                mImScreen.Source = mBitmapSource;
                mCanvas.Children.Add(mImScreen);
            } else {
                draw();
            }
            Primitive primitive;

            switch (ope) {
                case OPERATION.line:
                    if (locList.Count == 1) {
                        primitive = mDataManage.createLine(locList[0], lastPoint);
                        primitive.draw2D(mGDraw, mFace);
                    }
                    break;
                case OPERATION.circle:
                    if (locList.Count == 1) {
                        primitive = mDataManage.createCircle(locList[0], lastPoint);
                        primitive.draw2D(mGDraw, mFace);
                    }
                    break;
                case OPERATION.arc:
                    if (locList.Count == 2) {
                        primitive = mDataManage.createArc(locList[0], lastPoint, locList[1]);
                        primitive.draw2D(mGDraw, mFace);
                    } else if (locList.Count == 1) {
                        primitive = mDataManage.createLine(locList[0], lastPoint);
                        primitive.draw2D(mGDraw, mFace);
                    }
                    break;
                case OPERATION.polyline:
                    if (0 < locList.Count) {
                        List<PointD> plist = locList.ConvertAll(p => p);
                        plist.Add(lastPoint);
                        primitive = mDataManage.createPolyline(plist);
                        primitive.draw2D(mGDraw, mFace);
                    }
                    break;
                case OPERATION.rect:
                    if (locList.Count == 1) {
                        List<PointD> plist = new List<PointD>() {
                            locList[0], new PointD(locList[0].x, lastPoint.y),
                            lastPoint, new PointD(lastPoint.x, locList[0].y)
                        };
                        primitive = mDataManage.createPolygon(plist);
                        primitive.draw2D(mGDraw, mFace);
                    }
                    break;
                case OPERATION.polygon:
                    if (0 < locList.Count) {
                        List<PointD> plist = locList.ConvertAll(p => p);
                        plist.Add(lastPoint);
                        primitive = mDataManage.createPolygon(plist);
                        primitive.draw2D(mGDraw, mFace);
                    }
                    break;
                case OPERATION.translate:
                case OPERATION.copyTranslate:
                    for (int i = 1; i <= locList.Count; i++) {
                        PointD sp = i < locList.Count ? locList[i] : lastPoint;
                        Point3D v = new Point3D(sp, mFace) - new Point3D(locList[0], mFace);
                        foreach (var pick in pickData) {
                            primitive = mDataManage.mElementList[pick.mElementNo].mPrimitive.toCopy();
                            primitive.translate(v, pick.mPos, mFace);
                            primitive.createVertexData();
                            primitive.draw2D(mGDraw, mFace);
                        }
                    }
                    break;
                case OPERATION.rotate:
                case OPERATION.copyRotate:
                    for (int i = 2; i <= locList.Count; i++) {
                        PointD sp = i < locList.Count ? locList[i] : lastPoint;
                        double ang = locList[0].angle2(locList[1], sp);
                        Point3D cp = new Point3D(locList[0], mFace);
                        foreach (var pick in pickData) {
                            primitive = mDataManage.mElementList[pick.mElementNo].mPrimitive.toCopy();
                            primitive.rotate(cp, -ang, pick.mPos, mFace);
                            primitive.createVertexData();
                            primitive.draw2D(mGDraw, mFace);
                        }
                    }
                    break;
                case OPERATION.offset:
                case OPERATION.copyOffset:
                    for (int i = 1; i <= locList.Count; i++) {
                        PointD sp = i < locList.Count ? locList[i] : lastPoint;
                        if (0 < locList[0].length(sp)) {
                            foreach (var pick in pickData) {
                                primitive = mDataManage.mElementList[pick.mElementNo].mPrimitive.toCopy();
                                primitive.offset(new Point3D(locList[0], mFace), new Point3D(sp, mFace), pick.mPos, mFace);
                                primitive.createVertexData();
                                primitive.draw2D(mGDraw, mFace);
                            }
                        }
                    }
                    break;
                case OPERATION.mirror:
                case OPERATION.copyMirror:
                    if (locList.Count == 1) {
                        foreach (var pick in pickData) {
                            primitive = mDataManage.mElementList[pick.mElementNo].mPrimitive.toCopy();
                            primitive.mirror(new Point3D(locList[0], mFace), new Point3D(lastPoint, mFace), pick.mPos, mFace);
                            primitive.createVertexData();
                            primitive.draw2D(mGDraw, mFace);
                        }
                    }
                    break;
                case OPERATION.trim:
                case OPERATION.copyTrim:
                    if (locList.Count == 1) {
                        foreach (var pick in pickData) {
                            primitive = mDataManage.mElementList[pick.mElementNo].mPrimitive.toCopy();
                            primitive.trim(new Point3D(locList[0], mFace), new Point3D(lastPoint, mFace), pick.mPos, mFace);
                            primitive.createVertexData();
                            primitive.draw2D(mGDraw, mFace);
                        }
                    }
                    break;
                case OPERATION.scale:
                case OPERATION.copyScale:
                    for (int i = 2; i <= locList.Count; i++) {
                        PointD sp = i < locList.Count ? locList[i] : lastPoint;
                        double scale = locList[0].length(sp) / locList[0].length(locList[1]);
                        foreach (var pick in pickData) {
                            primitive = mDataManage.mElementList[pick.mElementNo].mPrimitive.toCopy();
                            primitive.scale(new Point3D(locList[0], mFace), scale, pick.mPos, mFace);
                            primitive.createVertexData();
                            primitive.draw2D(mGDraw, mFace);
                        }
                    }
                    break;
                case OPERATION.stretch:
                case OPERATION.stretchArc:
                    if (locList.Count == 1) {
                        Point3D v = new Point3D(lastPoint, mFace) - new Point3D(locList[0], mFace);
                        foreach (var pick in pickData) {
                            primitive = mDataManage.mElementList[pick.mElementNo].mPrimitive.toCopy();
                            primitive.stretch(v, ope == OPERATION.stretchArc, pick.mPos, mFace);
                            primitive.createVertexData();
                            primitive.draw2D(mGDraw, mFace);
                        }
                    }
                    break;
                case OPERATION.extrusion:
                    if (locList.Count == 1) {
                        Point3D v = new Point3D(lastPoint, mFace) - new Point3D(locList[0], mFace);
                        foreach (var pick in pickData) {
                            primitive = mDataManage.createExtrusion(mDataManage.mElementList[pick.mElementNo].mPrimitive, v);
                            primitive.draw2D(mGDraw, mFace);
                        }
                    }
                    break;
                case OPERATION.pasteElement:
                    if (locList.Count == 0 && mDataManage.mCopyArea != null) {
                        Point3D v = new Point3D(lastPoint, mFace) - mDataManage.mCopyArea.mMin;
                        Box b = mDataManage.mCopyArea.toBox(mFace);
                        b.offset(v.toPoint(mFace));
                        mGDraw.drawWRectangle(b);
                    }
                    break;
            }
            mGDraw.mPointType = 2;
            mGDraw.mPointSize = 2;
            mGDraw.drawWPoint(lastPoint);
        }

        /// <summary>
        /// ワールドウィンドウの設定
        /// </summary>
        /// <param name="viewSize">ビューサイズ</param>
        /// <param name="worldSize">ワールドサイズ</param>
        public void set3DWorldWindow(System.Windows.Size viewSize, double worldSize)
        {
            mGDraw.setViewSize(viewSize.Width, viewSize.Height);
            mGDraw.mAspectFix = true;
            mGDraw.mClipping = true;
            mGDraw.setWorldWindow(-worldSize * 1.1, worldSize * 1.1, worldSize * 1.1, -worldSize * 1.1);
        }

        /// <summary>
        /// 2Dの画面初期化
        /// </summary>
        public void dispInit()
        {
            mGDraw.clear();
            mGDraw.mFillColor = mBaseBackColor;
            mGDraw.mBrush = null;
            if (mGDraw.mFillColor != null)
                mGDraw.drawRectangle(mGDraw.mView);

            mGDraw.mFillColor = null;
            mGDraw.mBrush = Brushes.Black;
        }

        /// <summary>
        /// データの表示
        /// </summary>
        /// <param name="init">初期化</param>
        /// <param name="grid">グリッド表示</param>
        /// <param name="bitmap"ビットマップ取得></param>
        public void draw(bool init = true, bool grid = true, bool bitmap = true)
        {
            if (init)
                dispInit();
            draw2D(grid, bitmap);
        }

        /// <summary>
        /// 全体表示
        /// </summary>
        public void dispFit()
        {
            if (mFace == FACE3D.NON || mDataManage.mArea == null || mDataManage.mArea.isNaN())
                return;
            mGDraw.setWorldWindow(mDataManage.mArea.toBox(mFace));
            draw();
        }

        /// <summary>
        /// 2Dデータの表示
        /// </summary>
        /// <param name="grid">グリッド表示</param>
        /// <param name="bitmap">ビットマップ取得</param>
        public void draw2D(bool grid = true, bool bitmap = true)
        {
            if (grid)
                dispGrid(mGridSize);
            mLocPick.setPick();            //  ピック色設定
            for (int i = 0; i < mDataManage.mElementList.Count; i++) {
                if (mDataManage.mElementList[i].isDraw(mDataManage.mLayer))
                    mDataManage.mElementList[i].draw2D(mGDraw, mFace);
            }
            mLocPick.pickReset();           //  ピック食解除
            if (bitmap && mCanvas != null) {
                mBitmapSource = ylib.canvas2Bitmap(mCanvas);
                mBitmapOn = true;
            } else {
                mBitmapOn = false;
            }
        }

        /// <summary>
        /// キー操作による2D表示処理
        /// </summary>
        /// <param name="key">キーコード</param>
        /// <param name="control">Ctrlキー</param>
        /// <param name="shift">Shiftキー</param>
        public void key2DMove(Key key, bool control, bool shift)
        {
            if (control) {
                switch (key) {
                    case Key.F1: mGridSize *= -1; draw(); break;                    //  グリッド表示切替
                    case Key.Left: scroll(mScrollSize, 0); break;
                    case Key.Right: scroll(-mScrollSize, 0); break;
                    case Key.Up: scroll(0, mScrollSize); break;
                    case Key.Down: scroll(0, -mScrollSize); break;
                    case Key.PageUp: zoom(mGDraw.mWorld.getCenter(), 1.1); break;
                    case Key.PageDown: zoom(mGDraw.mWorld.getCenter(), 1 / 1.1); break;
                    default:
                        break;
                }
            } else if (shift) {
                switch (key) {
                    default: break;
                }
            } else {
                switch (key) {
                    case Key.F1: draw(true); break;                                 //  再表示
                    case Key.F2:                                                    //  領域拡大
                        mMainWindow.mPrevOpeMode = mMainWindow.mOperationMode;
                        mMainWindow.mOperationMode = OPEMODE.areaDisp;
                        break;
                    case Key.F3: dispFit(); break;                                  //  全体表示
                    case Key.F4: zoom(mGDraw.mWorld.getCenter(), 1.2); break;       //  拡大表示
                    case Key.F5: zoom(mGDraw.mWorld.getCenter(), 1 / 1.2); break;   //  縮小表示
                    //case Key.F6: dispWidthFit(); break;                           //  全幅表示
                    case Key.F7:                                                    //  領域ピック
                        mMainWindow.mPrevOpeMode = mMainWindow.mOperationMode;
                        mMainWindow.mOperationMode = OPEMODE.areaPick;
                        break;
                    default: break;
                }
            }
        }

        /// <summary>
        /// 領域指定操作
        /// </summary>
        /// <param name="wp">領域の中心座標</param>
        /// <param name="opeMode">操作モード</param>
        /// <returns>操作結果</returns>
        public bool areaOpe(PointD wp, OPEMODE opeMode)
        {
            if (mAreaLoc[0].isNaN()) {
                //  領域指定開始
                mAreaLoc[0] = wp;
            } else {
                //  領域決定
                if (1 < mAreaLoc.Count) {
                    Box dispArea = new Box(mAreaLoc[0], mAreaLoc[1]);
                    dispArea.normalize();
                    if (1 < mGDraw.world2screenXlength(dispArea.Width)) {
                        if (opeMode == OPEMODE.areaDisp) {
                            //  領域拡大表示
                            areaDisp(dispArea);
                        } else if (opeMode == OPEMODE.areaPick) {
                            //  領域ピック
                            PointD pickPos = dispArea.getCenter();
                            List<int> picks = mLocPick.getPickNo(dispArea);
                            mLocPick.pickElement(dispArea.getCenter(), picks, opeMode);
                            //  ピック色表示
                            mLocPick.setPick();
                            draw();
                            mLocPick.pickReset();
                        }
                    }
                    mAreaLoc[0] = new PointD();
                    mAreaLoc[1] = new PointD();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 2D表示の上下左右スクロール
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public void scroll(double dx, double dy)
        {
            PointD v = new PointD(mGDraw.screen2worldXlength(dx), mGDraw.screen2worldYlength(dy));
            mGDraw.mWorld.offset(v.inverse());

            if (mImScreen == null || mBitmapSource == null || !mBitmapOn) {
                //  全体再表示
                mGDraw.mClipBox = mGDraw.mWorld;
                draw(true, true);
                return;
            }

            //  ポリゴンの塗潰しで境界線削除ためオフセットを設定
            double offset = mGDraw.screen2worldXlength(2);

            dispInit();
            mGDraw.mClipBox = mGDraw.mWorld.toCopy();

            //  横空白部分を描画
            if (0 > dx) {
                mGDraw.mClipBox.Left = mGDraw.mWorld.Right + v.x - offset;
                mGDraw.mClipBox.Width = -v.x + offset;
            } else if (0 < dx) {
                mGDraw.mClipBox.Width = v.x + offset;
            }
            if (dx != 0) {
                draw2D(true, false);
            }

            //  縦空白部分を描画
            mGDraw.mClipBox = mGDraw.mWorld.toCopy();
            if (0 > dy) {
                mGDraw.mClipBox.Top -= mGDraw.mWorld.Height - v.y - offset;
                mGDraw.mClipBox.Height = v.y + offset;
            } else if (0 < dy) {
                mGDraw.mClipBox.Height = -v.y + offset;
            }
            if (dy != 0) {
                draw2D(true, false);
            }

            //  移動した位置にBitmapの貼付け(ポリゴン塗潰しの境界線削除でoffsetを設定)
            ylib.moveImage(mCanvas, mBitmapSource, dx, dy, 1);

            //  Windowの設定を元に戻す
            mGDraw.mClipBox = mGDraw.mWorld.toCopy();
            mBitmapSource = ylib.canvas2Bitmap(mCanvas);
            mBitmapOn = true;

            //  コピーしたイメージを貼り付けなおすことで文字のクリッピングする
            //mGDraw.clear();
            //moveImage(mCanvas, mBitmapSource, 0, 0);
        }

        /// <summary>
        /// 2D表示の拡大縮小
        /// </summary>
        /// <param name="wp">拡大縮小の中心座標</param>
        /// <param name="scaleStep">拡大率</param>
        public void zoom(PointD wp, double scaleStep)
        {
            mGDraw.setWorldZoom(wp, scaleStep, true);
            mGDraw.mClipBox = mGDraw.mWorld;
            draw();
        }

        /// <summary>
        /// 領域指定の2D表示
        /// </summary>
        /// <param name="area">領域座標</param>
        public void areaDisp(Box area)
        {
            mGDraw.setWorldWindow(area);
            draw();
        }

        /// <summary>
        /// 領域枠のドラッギング
        /// </summary>
        /// <param name="loc">座標リスト</param>
        public void areaDragging(List<PointD> loc)
        {
            mCanvas.Children.Clear();
            if (mBitmapSource != null && mBitmapOn) {
                mImScreen.Source = mBitmapSource;
                mCanvas.Children.Add(mImScreen);
            } else {
                draw();
            }

            mGDraw.mBrush = Brushes.Green;
            mGDraw.mFillColor = null;
            Box b = new Box(loc[0], loc[1]);
            List<PointD> plist = b.ToPointList();
            mGDraw.drawWPolygon(plist);
        }

        /// <summary>
        /// 2Dのフレームを表示
        /// </summary>
        public void drawWorldFrame()
        {
            //  背景色と枠の表示
            mGDraw.clear();
            mGDraw.mFillColor = mBaseBackColor;
            mGDraw.mBrush = Brushes.Black;

            mGDraw.setViewSize(new System.Windows.Size(mCanvas.ActualWidth, mCanvas.ActualHeight));
            Box world = mGDraw.mWorld.toCopy();
            world.scale(world.getCenter(), 0.99);
            Rect rect = new Rect(world.TopLeft.toPoint(), world.BottomRight.toPoint());
            mGDraw.drawWRectangle(rect);
        }

        /// <summary>
        /// グリッドの表示
        /// グリッド10個おきに大玉を表示
        /// </summary>
        /// <param name="size">グリッドの間隔</param>
        public void dispGrid(double size)
        {
            if (0 < size && size < 1000) {
                mGDraw.mBrush = mGDraw.getColor("Black");
                mGDraw.mThickness = 1.0;
                mGDraw.mPointType = 0;
                while (mGridMinmumSize > mGDraw.world2screenXlength(size) && size < 1000) {
                    size *= 10;
                }
                if (mGridMinmumSize <= mGDraw.world2screenXlength(size)) {
                    //  グリッド間隔(mGridMinmumSize)dot以上を表示
                    double y = mGDraw.mWorld.Bottom - size;
                    y = Math.Floor(y / size) * size;
                    while (y < mGDraw.mWorld.Top) {
                        double x = mGDraw.mWorld.Left;
                        x = Math.Floor(x / size) * size;
                        while (x < mGDraw.mWorld.Right) {
                            PointD p = new PointD(x, y);
                            if (x % (size * 10) == 0 && y % (size * 10) == 0) {
                                //  10個おきの点
                                mGDraw.mPointSize = 2;
                                mGDraw.drawWPoint(p);
                            } else {
                                mGDraw.mPointSize = 1;
                                mGDraw.drawWPoint(p);
                            }
                            x += size;
                        }
                        y += size;
                    }
                }
            }
            //  原点(0,0)表示
            mGDraw.mBrush = mGDraw.getColor("Red");
            mGDraw.mPointType = 2;
            mGDraw.mPointSize = 5;
            mGDraw.drawWPoint(new PointD(0, 0));
        }

        /// <summary>
        /// 表示属性を文字列配列リストに変換
        /// </summary>
        /// <returns></returns>
        public List<string[]> toDataList()
        {
            List<string[]> list = new List<string[]>();
            string[] buf = { "DataDraw" };
            list.Add(buf);
            buf = new string[] { "WorldSize", mWorldSize.ToString() };
            list.Add(buf);
            buf = new string[] { "GridSize", mGridSize.ToString() };
            list.Add(buf);
            if (mBaseBackColor != null) {
                buf = new string[] { "BaseBackColor", ylib.getBrushName(mBaseBackColor) };
                list.Add(buf);
            }
            buf = new string[] {"World",
                mGDraw.mWorld.Left.ToString(), mGDraw.mWorld.Top.ToString(),
                mGDraw.mWorld.Right.ToString(), mGDraw.mWorld.Bottom.ToString()
            };
            list.Add(buf);
            buf = new string[] { "DataDrawEnd" };
            list.Add(buf);
            return list;
        }

        /// <summary>
        /// 表示属性を設定
        /// </summary>
        /// <param name="dataList">文字列配列リスト</param>
        /// <param name="sp">リスト開始位置</param>
        /// <returns>リスト終了位置</returns>
        public int setDataList(List<string[]> dataList, int sp)
        {
            mBaseBackColor = null;
            while (sp < dataList.Count) {
                string[] buf = dataList[sp++];
                if (buf[0] == "BaseBackColor") {
                    mBaseBackColor = ylib.getBrsh(buf[1]);
                } else if (buf[0] == "WorldSize") {
                    mWorldSize = ylib.doubleParse(buf[1]);
                } else if (buf[0] == "GridSize") {
                    mGridSize = ylib.doubleParse(buf[1]);
                } else if (buf[0] == "World") {
                    mGDraw.mWorld.Left   = ylib.doubleParse(buf[1]);
                    mGDraw.mWorld.Top    = ylib.doubleParse(buf[2]);
                    mGDraw.mWorld.Right  = ylib.doubleParse(buf[3]);
                    mGDraw.mWorld.Bottom = ylib.doubleParse(buf[4]);
                } else if (buf[0] == "DataDrawEnd") {
                    break;
                }
            }
            return sp;
        }

        /// <summary>
        /// 画面コピー
        /// </summary>
        public void screenCopy()
        {
            BitmapSource bitmapSource = toBitmapScreen();
            System.Windows.Clipboard.SetImage(bitmapSource);
        }

        /// <summary>
        /// 作図領域のコピー
        /// </summary>
        /// <returns>BitmapSource</returns>
        public BitmapSource toBitmapScreen()
        {
            Brush tmpColor = mBaseBackColor;
            mBaseBackColor = Brushes.White;
            draw(true, false, false);
            BitmapSource bitmapSource = ylib.canvas2Bitmap(mCanvas);
            mBaseBackColor = tmpColor;
            return bitmapSource;
        }
    }
}
