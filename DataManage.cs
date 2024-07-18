using CoreLib;
using System.IO;
using System.Windows;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Mini3DCad
{
    /// <summary>
    /// データ管理クラス
    /// </summary>
    public class DataManage
    {
        public double mArcDivideAng = Math.PI / 12;                     //  円弧の分割角度
        public double mRevolutionDivideAng = Math.PI / 18;              //  回転体の分割角度
        public double mSweepDivideAng = Math.PI / 9;                    //  掃引(スィープ)の回転分割角度
        public bool mSurfaceVertex = false;                             //  Debug用(Polygon分割表示)
        public bool mWireFrame = false;                                 //  ワイヤーフレーム表示
        public double mFilletSize = 0;                                  //  R面取り(フィレット)半径

        public FACE3D mFace = FACE3D.XY;                                //  Primitive 作成面
        public Brush mPrimitiveBrush = Brushes.Green;                   //  Primitiveの色設定
        public List<Element> mElementList = new List<Element>();        //  エレメントリスト
        public List<Element> mCopyElementList = new List<Element>();    //  エレメントリスト
        public Box3D mArea;                                             //  要素領域
        public Box3D mCopyArea;                                         //  要素領域
        public int mOperationCount = 0;                                 //  操作回数
        public List<string> mCommandHistory = new List<string>();       //  コマンド履歴
        public int mFirstEntityCount = 0;                               //  編集開始時の要素数
        public int mLayerSize = 64;                                     //  レイヤサイズ
        public Layer mLayer;                                            //  レイヤ
        public string mZumenComment;                                    //  図面コメント

        public List<string[]> mImageFilters = new List<string[]>() {
                    new string[] { "PNGファイル", "*.png" },
                    new string[] { "JPGファイル", "*.jpg" },
                    new string[] { "JPEGファイル", "*.jpeg" },
                    new string[] { "GIFファイル", "*.gif" },
                    new string[] { "BMPファイル", "*.bmp" },
                    new string[] { "すべてのファイル", "*.*"}
                };

        //  XYZ軸データ
        private List<Point3D> mXAxis = new List<Point3D>() { new Point3D(0, 0, 0), new Point3D(1, 0, 0) };
        private List<Point3D> mYAxis = new List<Point3D>() { new Point3D(0, 0, 0), new Point3D(0, 1, 0) };
        private List<Point3D> mZAxis = new List<Point3D>() { new Point3D(0, 0, 0), new Point3D(0, 0, 1) };

        private MainWindow mMainWindow;
        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mainWindow">MainWindow</param>
        public DataManage(MainWindow mainWindow)
        {
            mMainWindow = mainWindow;
            mLayer = new Layer(mLayerSize);
        }

        /// <summary>
        /// Elementデータリストをクリア
        /// </summary>
        public void clear()
        {
            mElementList.Clear();
            mZumenComment = "";
            mOperationCount = 0;
            mFirstEntityCount = 0;
            mLayer.clear();
        }

        /// <summary>
        /// データの登録
        /// </summary>
        /// <param name="ope">処理の種別</param>
        /// <param name="last">ロケイトの完了</param>
        /// <returns></returns>
        public bool defineData(OPERATION ope, List<PointD> locList, List<PickData> pickElement, bool last = false)
        {
            mOperationCount++;
            switch (ope) {
                case OPERATION.point:
                    if (locList.Count == 1) {
                        addPoint(locList[0]);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.line:
                    if (locList.Count == 2) {
                        addLine(locList[0], locList[1]);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.circle:
                    if (locList.Count == 2) {
                        addCircle(locList[0], locList[1]);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.arc:
                    if (locList.Count == 3) {
                        addArc(locList[0], locList[2], locList[1]);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.polyline:
                    if (1 < locList.Count && last) {
                        addPolyline(locList);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.rect:
                    if (locList.Count == 2) {
                        addRect(locList[0], locList[1]);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.polygon:
                    if (2 < locList.Count && last) {
                        addPolygon(locList);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.translate:
                    if (locList.Count == 2 && 0 < pickElement.Count) {
                        translate(pickElement, locList);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.copyTranslate:
                    if (1 < locList.Count && last && 0 < pickElement.Count) {
                        translate(pickElement, locList, true);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.rotate:
                    if (locList.Count == 3 && 0 < pickElement.Count) {
                        rotate(pickElement, locList);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.copyRotate:
                    if (2 < locList.Count && last && 0 < pickElement.Count) {
                        rotate(pickElement, locList, true);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.offset:
                    if (locList.Count == 2 && 0 < pickElement.Count) {
                        offset(pickElement, locList);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.copyOffset:
                    if (1 < locList.Count && last && 0 < pickElement.Count) {
                        offset(pickElement, locList, true);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.mirror:
                    if (locList.Count == 2 && 0 < pickElement.Count) {
                        mirror(pickElement, locList[0], locList[1]);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.copyMirror:
                    if (locList.Count == 2 && 0 < pickElement.Count) {
                        mirror(pickElement, locList[0], locList[1], true);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.trim:
                    if (locList.Count == 2 && 0 < pickElement.Count) {
                        trim(pickElement, locList[0], locList[1]);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.copyTrim:
                    if (locList.Count == 2 && 0 < pickElement.Count) {
                        trim(pickElement, locList[0], locList[1], true);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.scale:
                    if (locList.Count == 3 && 0 < pickElement.Count) {
                        scale(pickElement, locList);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.copyScale:
                    if (2 < locList.Count && last && 0 < pickElement.Count) {
                        scale(pickElement, locList, true);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.stretch:
                case OPERATION.stretchArc:
                    if (locList.Count == 2 && 0 < pickElement.Count) {
                        stretch(pickElement, locList, ope == OPERATION.stretchArc);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.divide:
                    if (locList.Count == 1 && 0 < pickElement.Count) {
                        divide(pickElement, locList);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.extrusion:
                    if (locList.Count == 2 && 0 < pickElement.Count) {
                        extrusion(pickElement, locList[0], locList[1]);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.revolution:
                    if (1 < pickElement.Count) {
                        revolution(pickElement);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.pasteElement:
                    if (locList.Count == 1) {
                        pasteElement(locList[0]);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.measureAngle:
                    if (locList.Count == 3) {
                        measure(locList);
                        locList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.measureDistance:
                    if (locList.Count == 2) {
                        measure(locList);
                        locList.Clear();
                    } else
                        return false;
                    break;
                default:
                    return false;
            }
            updateData();
            return true;
        }

        /// <summary>
        /// 点要素の追加
        /// </summary>
        /// <param name="p">点座標</param>
        public void addPoint(PointD p)
        {
            addPoint(new Point3D(p, mFace));
        }

        /// <summary>
        /// 点要素の追加
        /// </summary>
        /// <param name="p">点座標</param>
        public void addPoint(Point3D p)
        {
            Element element = new Element(mLayerSize);
            element.mName = "点";
            element.mPrimitive = createPoint(p);
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
            mCommandHistory.Add(element.mPrimitive.toCommand());
        }

        /// <summary>
        /// 線分追加
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public void addLine(PointD sp, PointD ep)
        {
            addLine(new Point3D(sp, mFace), new Point3D(ep, mFace));
        }

        /// <summary>
        /// 線分追加
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public void addLine(Point3D sp, Point3D ep)
        {
            Element element = new Element(mLayerSize);
            element.mName = "線分";
            element.mPrimitive = createLine(new Line3D(sp, ep));
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
            mCommandHistory.Add(element.mPrimitive.toCommand());
        }

        /// <summary>
        /// 円の追加
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="ep">円弧上の点</param>
        public void addCircle(PointD cp, PointD ep)
        {
            Arc3D arc3D = new Arc3D(new ArcD(cp, ep, Math.PI * 2), mFace);
            addArc(arc3D);
        }

        /// <summary>
        /// 円の追加
        /// </summary>
        /// <param name="arc">円データ</param>
        public void addCircle(Arc3D arc)
        {
            Element element = new Element(mLayerSize);
            element.mName = "円";
            arc.mSa = 0;
            arc.mEa = Math.PI * 2;
            element.mPrimitive = createArc(arc);
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
            mCommandHistory.Add(element.mPrimitive.toCommand());
        }

        /// <summary>
        /// 円弧の追加
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="mp">中間点</param>
        /// <param name="ep">終点</param>
        public void addArc(PointD sp, PointD mp, PointD ep)
        {
            Arc3D arc3D = new Arc3D(new ArcD(sp, mp, ep), mFace);
            addArc(arc3D);
        }

        /// <summary>
        /// 円弧の追加
        /// </summary>
        /// <param name="arc">円弧</param>
        public void addArc(Arc3D arc)
        {
            try {
                Element element = new Element(mLayerSize);
                element.mName = "円弧";
                element.mPrimitive = createArc(arc);
                element.mOperationNo = mOperationCount;
                element.update3DData();
                mElementList.Add(element);
                mCommandHistory.Add(element.mPrimitive.toCommand());
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"addArc: {e.Message}");
            }
        }

        /// <summary>
        /// ポリラインの追加
        /// </summary>
        /// <param name="plist">2D座標点リスト</param>
        public void addPolyline(List<PointD> plist)
        {
            Element element = new Element(mLayerSize);
            element.mName = "ポリライン";
            element.mPrimitive = createPolyline(plist);
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
            mCommandHistory.Add(element.mPrimitive.toCommand());
        }

        /// <summary>
        /// ポリラインの追加
        /// </summary>
        /// <param name="plist">3D座標点リスト</param>
        /// <param name="color">線分カラー</param>
        /// <param name="faceColor">フェイスカラー</param>
        public void addPolyline(List<Point3D> plist, Brush color = null, Brush faceColor = null, double divAng = 0)
        {
            Polyline3D polyline = new Polyline3D(plist);
            addPolyline(polyline, color, faceColor, divAng);
        }

        /// <summary>
        /// ポリラインの追加
        /// </summary>
        /// <param name="polygon">ポリライン</param>
        /// <param name="color">線分カラー</param>
        /// <param name="faceColor">フェイスカラー</param>
        public void addPolyline(Polyline3D polyline, Brush color = null, Brush faceColor = null, double divAng = 0)
        {
            Element element = new Element(mLayerSize);
            element.mName = "ポリライン";
            element.mPrimitive = createPolyline(polyline);
            if (color != null)
                element.mPrimitive.mLineColor = color;
            if (faceColor != null)
                element.mPrimitive.mFaceColors[0] = faceColor;
            if (0 < divAng)
                element.mPrimitive.mDivideAngle = divAng;
            element.mPrimitive.createVertexData();
            element.mPrimitive.createSurfaceData();
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
            mCommandHistory.Add(element.mPrimitive.toCommand());
        }

        /// <summary>
        /// 四角形(Polygon)の追加
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public void addRect(PointD sp, PointD ep)
        {
            if (sp.x == ep.x || sp.y == ep.y)
                return;
            List<PointD> plist = new List<PointD>() {
                sp, new PointD(sp.x, ep.y), ep, new PointD(ep.x, sp.y) };
            Element element = new Element(mLayerSize);
            element.mName = "四角形";
            element.mPrimitive = createPolygon(plist);
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
            mCommandHistory.Add(element.mPrimitive.toCommand());
        }

        /// <summary>
        /// ポリゴンの追加
        /// </summary>
        /// <param name="pist">座標点リスト</param>
        public void addPolygon(List<PointD> plist)
        {
            Polygon3D polygon = new Polygon3D(plist, mFace);
            addPolygon(polygon);
        }

        /// <summary>
        /// ポリゴンの追加
        /// </summary>
        /// <param name="pist"></param>
        public void addPolygon(List<Point3D> plist)
        {
            Polygon3D polygon = new Polygon3D(plist);
            addPolygon(polygon);
        }

        /// <summary>
        /// ポリゴンの追加
        /// </summary>
        /// <param name="polygon">ポリゴン</param>
        /// <param name="color">線分カラー</param>
        /// <param name="faceColor">フェイスカラー</param>
        public void addPolygon(Polygon3D polygon, Brush color = null, Brush faceColor = null, double divAng = 0)
        {
            Element element = new Element(mLayerSize);
            element.mName = "ポリゴン";
            element.mPrimitive = createPolygon(polygon);
            if (color != null)
                element.mPrimitive.mLineColor = color;
            if (faceColor != null)
                element.mPrimitive.mFaceColors[0] = faceColor;
            if (0 < divAng)
                element.mPrimitive.mDivideAngle = divAng;
            element.mPrimitive.createVertexData();
            element.mPrimitive.createSurfaceData();
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
            mCommandHistory.Add(element.mPrimitive.toCommand());
        }

        /// <summary>
        /// Link要素を追加
        /// </summary>
        /// <param name="linkNo">リンク先No</param>
        public void addLink(int linkNo)
        {
            Element element = new Element(mLayerSize);
            element.mName = "リンク";
            element.mLinkNo = linkNo;
            element.mOperationNo = mOperationCount;
            mElementList.Add(element);
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="picks">ピック要素</param>
        /// <param name="locList">座標リスト</param>
        /// <param name="copy">コピーの有無</param>
        public void translate(List<PickData> picks, List<PointD> locList, bool copy = false)
        {
            for (int i = 1; i < locList.Count; i++) {
                Point3D v = new Point3D(locList[i], mFace) - new Point3D(locList[0], mFace);
                foreach (var pick in picks) {
                    Element element = mElementList[pick.mElementNo].toCopy();
                    element.mPrimitive.translate(v, pick.mPos, mFace);
                    element.mPrimitive.createSurfaceData();
                    element.mPrimitive.createVertexData();
                    element.mOperationNo = mOperationCount;
                    element.update3DData();
                    mElementList.Add(element);
                    if (!copy) {
                        mElementList[pick.mElementNo].mRemove = true;
                        addLink(pick.mElementNo);
                    }
                }
            }
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="picks">ピック要素</param>
        /// <param name="locList">座標リスト</param>
        /// <param name="copy">コピーの有無</param>
        public void rotate(List<PickData> picks, List<PointD> locList, bool copy = false)
        {
            for (int i = 2; i < locList.Count; i++) {
                double ang = locList[0].angle2(locList[1], locList[i]);
                Point3D cp = new Point3D(locList[0], mFace);
                foreach (var pick in picks) {
                    Element element = mElementList[pick.mElementNo].toCopy();
                    element.mPrimitive.rotate(cp, -ang, pick.mPos, mFace);
                    element.mPrimitive.createSurfaceData();
                    element.mPrimitive.createVertexData();
                    element.mOperationNo = mOperationCount;
                    element.update3DData();
                    mElementList.Add(element);
                    if (!copy) {
                        mElementList[pick.mElementNo].mRemove = true;
                        addLink(pick.mElementNo);
                    }
                }
            }
        }

        /// <summary>
        /// オフセット
        /// </summary>
        /// <param name="picks">ピック要素</param>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="copy">コピーの有無</param>
        public void offset(List<PickData> picks, List<PointD> locList, bool copy = false)
        {
            PointD sp = locList[0];
            for (int i = 1; i < locList.Count; i++) {
                PointD ep = locList[i];
                foreach (var pick in picks) {
                    Element element = mElementList[pick.mElementNo].toCopy();
                    if (element.mPrimitive.mPrimitiveId == PrimitiveId.Point ||
                        element.mPrimitive.mPrimitiveId == PrimitiveId.Line ||
                        element.mPrimitive.mPrimitiveId == PrimitiveId.Arc ||
                        element.mPrimitive.mPrimitiveId == PrimitiveId.Polyline ||
                        element.mPrimitive.mPrimitiveId == PrimitiveId.Polygon ||
                        element.mPrimitive.mPrimitiveId == PrimitiveId.Extrusion) {
                        element.mPrimitive.offset(new Point3D(sp, mFace), new Point3D(ep, mFace), pick.mPos, mFace);
                        element.mPrimitive.createSurfaceData();
                        element.mPrimitive.createVertexData();
                        element.mOperationNo = mOperationCount;
                        element.update3DData();
                        mElementList.Add(element);
                        if (!copy) {
                            mElementList[pick.mElementNo].mRemove = true;
                            addLink(pick.mElementNo);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ミラー(反転)
        /// </summary>
        /// <param name="picks">ピック要素</param>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="copy">コピーの有無</param>
        public void mirror(List<PickData> picks, PointD sp, PointD ep, bool copy = false)
        {
            Point3D ps = new Point3D(sp, mFace);
            Point3D pe = new Point3D(ep, mFace);
            foreach (var pick in picks) {
                Element element = mElementList[pick.mElementNo].toCopy();
                element.mPrimitive.mirror(ps, pe, pick.mPos, mFace);
                element.mPrimitive.createSurfaceData();
                element.mPrimitive.createVertexData();
                element.mOperationNo = mOperationCount;
                element.update3DData();
                mElementList.Add(element);
                if (!copy) {
                    mElementList[pick.mElementNo].mRemove = true;
                    addLink(pick.mElementNo);
                }
            }
        }

        /// <summary>
        /// トリム
        /// </summary>
        /// <param name="picks">ピック要素</param>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <param name="copy">コピーの有無</param>
        public void trim(List<PickData> picks, PointD sp, PointD ep, bool copy = false)
        {
            Point3D ps = new Point3D(sp, mFace);
            Point3D pe = new Point3D(ep, mFace);
            foreach (var pick in picks) {
                Element element = mElementList[pick.mElementNo].toCopy();
                element.mPrimitive.trim(ps, pe, pick.mPos, mFace);
                element.mPrimitive.createSurfaceData();
                element.mPrimitive.createVertexData();
                element.mOperationNo = mOperationCount;
                element.update3DData();
                mElementList.Add(element);
                if (!copy) {
                    mElementList[pick.mElementNo].mRemove = true;
                    addLink(pick.mElementNo);
                }
            }
        }

        /// <summary>
        /// 拡大縮小
        /// </summary>
        /// <param name="picks">ピック要素</param>
        /// <param name="locList">ロケイト位置</param>
        /// <param name="copy">コピー有無</param>
        public void scale(List<PickData> picks, List<PointD> locList, bool copy = false)
        {
            for (int i = 2; i < locList.Count; i++) {
                double scale = locList[0].length(locList[i]) / locList[0].length(locList[1]);
                Point3D cp = new Point3D(locList[0], mFace);
                foreach (var pick in picks) {
                    Element element = mElementList[pick.mElementNo].toCopy();
                    element.mPrimitive.scale(cp, scale, pick.mPos, mFace);
                    element.mPrimitive.createSurfaceData();
                    element.mPrimitive.createVertexData();
                    element.mOperationNo = mOperationCount;
                    element.update3DData();
                    mElementList.Add(element);
                    if (!copy) {
                        mElementList[pick.mElementNo].mRemove = true;
                        addLink(pick.mElementNo);
                    }
                }
            }
        }

        /// <summary>
        /// ストレッチ
        /// </summary>
        /// <param name="picks">ピック要素</param>
        /// <param name="locList">ロケイト位置</param>
        /// <param name="arc">円弧ストレッチ</param>
        /// <param name="copy">コピー有無</param>
        public void stretch(List<PickData> picks, List<PointD> locList, bool arc, bool copy = false)
        {
            if (1 < locList.Count) {
                Point3D v = new Point3D(locList[1], mFace) - new Point3D(locList[0], mFace);
                foreach (var pick in picks) {
                    Element element = mElementList[pick.mElementNo].toCopy();
                    element.mPrimitive.stretch(v, arc, pick.mPos, mFace);
                    element.mPrimitive.createSurfaceData();
                    element.mPrimitive.createVertexData();
                    element.mOperationNo = mOperationCount;
                    element.update3DData();
                    mElementList.Add(element);
                    if (!copy) {
                        mElementList[pick.mElementNo].mRemove = true;
                        addLink(pick.mElementNo);
                    }
                }
            }
        }

        /// <summary>
        /// 分割処理
        /// </summary>
        /// <param name="picks">ピックリスト</param>
        /// <param name="locList">ロケイトリスト</param>
        public void divide(List<PickData> picks, List<PointD> locList)
        {
            foreach (var pick in picks) {
                Element element = mElementList[pick.mElementNo].toCopy();
                bool result = false;
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Polyline) {
                    result = dividePolyline(element, locList[0]);
                } else if (element.mPrimitive.mPrimitiveId == PrimitiveId.Polygon) {
                    result = dividePolygon(element, locList[0]);
                } else if (element.mPrimitive.mPrimitiveId == PrimitiveId.Line) {
                    result = divideLine(element, locList[0]);
                } else if (element.mPrimitive.mPrimitiveId == PrimitiveId.Arc) {
                    result = divideArc(element, locList[0]);
                }
                if (result) {
                    mElementList[pick.mElementNo].mRemove = true;
                    addLink(pick.mElementNo);
                }
            }
        }

        /// <summary>
        /// フィレット(R面取り)処理
        /// </summary>
        /// <param name="picks">ピックリスト</param>
        public void fillet(List<PickData> picks)
        {
            if (picks.Count == 1) {
                Element ele0 = mElementList[picks[0].mElementNo].toCopy();
                bool result = false;
                if (ele0.mPrimitive.mPrimitiveId == PrimitiveId.Polyline ||
                    ele0.mPrimitive.mPrimitiveId == PrimitiveId.Polygon) {
                    result = fillet(ele0, picks[0].mPos);
                }
                if (result) {
                    mElementList[picks[0].mElementNo].mRemove = true;
                    addLink(picks[0].mElementNo);
                }
            } else if (picks.Count == 2) {

            } else {

            }
        }

        /// <summary>
        /// 結合処理(2要素を接続する))
        /// </summary>
        /// <param name="picks">ピックリスト</param>
        public void connect(List<PickData> picks)
        {
            if (picks.Count < 1)
                return;

            Element ele0 = mElementList[picks[0].mElementNo];
            if (picks.Count == 1 || picks[0].mElementNo == picks[1].mElementNo) {
                //  同一要素をピック ポリゴン化
                Polyline3D polyline = new Polyline3D();
                if (ele0.mPrimitive.mPrimitiveId == PrimitiveId.Polyline) {
                    PolylinePrimitive polylinePrimitive = (PolylinePrimitive)ele0.mPrimitive;
                    polyline = polylinePrimitive.mPolyline.toCopy();
                } else if (ele0.mPrimitive.mPrimitiveId == PrimitiveId.Arc) {
                    ArcPrimitive arcPrimitive = (ArcPrimitive)ele0.mPrimitive;
                    polyline = arcPrimitive.toPointList();
                } else
                    return;
                polyline.lastCrossCheck();
                polyline.squeeze();
                addPolygon(new Polygon3D(polyline), ele0.mPrimitive.mLineColor, ele0.mPrimitive.mFaceColors[0], ele0.mPrimitive.mDivideAngle);
                mElementList[^1].copyProperty(ele0);
                mElementList[^1].copyLayer(ele0);
            } else if (picks.Count == 2) {
                //  2要素をピック位置で接続
                Element ele1 = mElementList[picks[1].mElementNo];
                if ((ele0.mPrimitive.mPrimitiveId == PrimitiveId.Line ||
                    ele0.mPrimitive.mPrimitiveId == PrimitiveId.Arc ||
                    ele0.mPrimitive.mPrimitiveId == PrimitiveId.Polyline) &&
                    (ele1.mPrimitive.mPrimitiveId == PrimitiveId.Line ||
                    ele1.mPrimitive.mPrimitiveId == PrimitiveId.Arc ||
                    ele1.mPrimitive.mPrimitiveId == PrimitiveId.Polyline)) {

                    Polyline3D pl0 = ele0.mPrimitive.toPointList();
                    Polyline3D pl1 = ele1.mPrimitive.toPointList();
                    pl0.connect(new Point3D(picks[0].mPos, mFace), pl1, new Point3D(picks[1].mPos, mFace));
                    pl0.squeeze();
                    addPolyline(pl0, ele0.mPrimitive.mLineColor, ele0.mPrimitive.mFaceColors[0]);
                    mElementList[^1].copyProperty(ele0);
                    mElementList[^1].copyLayer(ele0);
                } else
                    return;
            } else if (2 < picks.Count) {
                //  3要素以上で端点接続
                Polyline3D polyline = new Polyline3D();
                for (int i = 0; i< picks.Count; i++) {
                    Element ele1 = mElementList[picks[i].mElementNo];
                    if (ele1.mPrimitive.mPrimitiveId == PrimitiveId.Line ||
                        ele1.mPrimitive.mPrimitiveId == PrimitiveId.Arc ||
                        ele1.mPrimitive.mPrimitiveId == PrimitiveId.Polyline) {
                        Polyline3D polyline1 = ele1.mPrimitive.toPointList();
                        polyline.connect(polyline1);
                    } else
                        return;
                }
                polyline.squeeze();
                addPolyline(polyline, ele0.mPrimitive.mLineColor, ele0.mPrimitive.mFaceColors[0]);
                mElementList[^1].copyProperty(ele0);
                mElementList[^1].copyLayer(ele0);
            } else
                return;

            //  undo用のリンクエレメント作成
            for (int i = 0; i < picks.Count; i++) {
                mElementList[picks[i].mElementNo].mRemove = true;
                addLink(picks[i].mElementNo);
            }
        }

        /// <summary>
        /// 線分要素に分解する
        /// </summary>
        /// <param name="picks">ピックリスト</param>
        public void disassemble(List<PickData> picks)
        {
            if (picks.Count < 1)
                return;
            foreach (var pick in picks) {
                Element ele = mElementList[pick.mElementNo];
                if (ele.mPrimitive.mPrimitiveId == PrimitiveId.Polyline) {
                    PolylinePrimitive polyline = (PolylinePrimitive)ele.mPrimitive;
                    List<Point3D> plist = polyline.mPolyline.toPoint3D();
                    for (int i = 0; i < plist.Count - 1; i++) {
                        if (plist[i + 1].type == 1) {
                            addArc(plist[i].toPoint(mFace), plist[i + 1].toPoint(mFace), plist[i + 2].toPoint(mFace));
                            i++;
                        } else
                            addLine(plist[i], plist[i + 1]);
                        Element lineEle = mElementList[^1];
                        lineEle.copyProperty(ele);
                        lineEle.copyLayer(ele);
                    }
                } else if (ele.mPrimitive.mPrimitiveId == PrimitiveId.Polygon) {
                    PolygonPrimitive polygon = (PolygonPrimitive)ele.mPrimitive;
                    List<Point3D> plist = polygon.mPolygon.toPoint3D();
                    for (int i = 0; i < plist.Count; i++) {
                        int ii = i % plist.Count;
                        int i1 = (i + 1) % plist.Count;
                        int i2 = (i + 2) % plist.Count;
                        if (plist[i1].type == 1) {
                            addArc(plist[ii].toPoint(mFace), plist[i1].toPoint(mFace), plist[i2].toPoint(mFace));
                            i++;
                        } else
                            addLine(plist[i], plist[i1]);
                        Element lineEle = mElementList[^1];
                        lineEle.copyProperty(ele);
                        lineEle.copyLayer(ele);
                    }
                } else
                    continue;
                //  undo用のリンクエレメント作成
                mElementList[pick.mElementNo].mRemove = true;
                addLink(pick.mElementNo);
            }
        }

        /// <summary>
        /// 押出処理
        /// </summary>
        /// <param name="pick">ピックデータ</param>
        /// <param name="sp">押出始点</param>
        /// <param name="ep">押出終点</param>
        public void extrusion(List<PickData> picks, PointD sp, PointD ep)
        {
            Point3D v = new Point3D(ep, mFace) - new Point3D(sp, mFace);
            foreach (var pick in picks) {
                ExtrusionPrimitive push = createExtrusion(mElementList[pick.mElementNo].mPrimitive, v);
                if (push == null)
                    return;
                Element element = new Element(mLayerSize);
                element.mName = mElementList[pick.mElementNo].mName;
                if (0 > element.mName.IndexOf("押出"))
                    element.mName += "-押出";
                element.copyLayer(mElementList[pick.mElementNo]);
                element.mPrimitive = push;
                element.mOperationNo = mOperationCount;
                element.update3DData();
                mElementList.Add(element);

                mElementList[pick.mElementNo].mRemove = true;
                addLink(pick.mElementNo);
            }
        }

        /// <summary>
        /// ブレンド処理
        /// </summary>
        /// <param name="picks">ピック要素</param>
        public void blend(List<PickData> picks)
        {
            if (picks.Count < 2)
                return;
            if (mElementList[picks[0].mElementNo].mPrimitive.mPrimitiveId == PrimitiveId.Polygon &&
                mElementList[picks[1].mElementNo].mPrimitive.mPrimitiveId == PrimitiveId.Polygon) {
                //  ポリゴン同士
                Polygon3D polygon1 = ((PolygonPrimitive)mElementList[picks[0].mElementNo].mPrimitive).mPolygon.toCopy();
                Polygon3D polygon2 = ((PolygonPrimitive)mElementList[picks[1].mElementNo].mPrimitive).mPolygon.toCopy();
                if (polygon1.isCounterClockWise(mFace)) polygon1.reverse();
                if (polygon2.isCounterClockWise(mFace)) polygon2.reverse();
                int n1 = polygon1.nearPosition(new Point3D(picks[0].mPos, mFace));
                int n2 = polygon2.nearPosition(new Point3D(picks[1].mPos, mFace));
                if (n1 < 0 || n2 < 0) return;
                polygon1.changeStart(n1);
                polygon2.changeStart(n2);

                BlendPrimitive blendPrimitive = new BlendPrimitive(polygon1, polygon2, mArcDivideAng);

                blendPrimitive.copyProperty(mElementList[picks[0].mElementNo].mPrimitive);
                blendPrimitive.createSurfaceData();
                blendPrimitive.createVertexData();
                Element element = new Element(mLayerSize);
                element.copyLayer(mElementList[picks[0].mElementNo]);
                element.mPrimitive = blendPrimitive;
                element.mName = mElementList[picks[0].mElementNo].mName;
                if (0 > element.mName.IndexOf("ブレンド"))
                    element.mName += "-ブレンド";
                element.mOperationNo = mOperationCount;
                element.update3DData();
                mElementList.Add(element);
            } else {
                //  ポリゴン以外
                Polyline3D polyline1;
                Polyline3D polyline2;
                if (mElementList[picks[0].mElementNo].mPrimitive.mPrimitiveId == PrimitiveId.Line) {
                    polyline1 = new Polyline3D(((LinePrimitive)mElementList[picks[0].mElementNo].mPrimitive).mLine);
                } else if (mElementList[picks[0].mElementNo].mPrimitive.mPrimitiveId == PrimitiveId.Arc) {
                    polyline1 = new Polyline3D(((ArcPrimitive)mElementList[picks[0].mElementNo].mPrimitive).mArc, 0);
                } else if (mElementList[picks[0].mElementNo].mPrimitive.mPrimitiveId == PrimitiveId.Polyline) {
                    polyline1 = ((PolylinePrimitive)mElementList[picks[0].mElementNo].mPrimitive).mPolyline.toCopy();
                } else
                    return;
                if (mElementList[picks[1].mElementNo].mPrimitive.mPrimitiveId == PrimitiveId.Line) {
                    polyline2 = new Polyline3D(((LinePrimitive)mElementList[picks[1].mElementNo].mPrimitive).mLine);
                } else if (mElementList[picks[1].mElementNo].mPrimitive.mPrimitiveId == PrimitiveId.Arc) {
                    polyline2 = new Polyline3D(((ArcPrimitive)mElementList[picks[1].mElementNo].mPrimitive).mArc, 0);
                } else if (mElementList[picks[1].mElementNo].mPrimitive.mPrimitiveId == PrimitiveId.Polyline) {
                    polyline2 = ((PolylinePrimitive)mElementList[picks[1].mElementNo].mPrimitive).mPolyline.toCopy();
                } else
                    return;
                double len1 = polyline1.length();
                double len2 = polyline2.length();
                if (polyline1.length(new Point3D(picks[0].mPos, mFace)) > len1 / 2)
                    polyline1.reverse();
                if (polyline2.length(new Point3D(picks[1].mPos, mFace)) > len2 / 2)
                    polyline2.reverse();

                BlendPolylinePrimitive blendPrimitive = new BlendPolylinePrimitive(polyline1, polyline2, mArcDivideAng);

                blendPrimitive.copyProperty(mElementList[picks[0].mElementNo].mPrimitive);
                blendPrimitive.createSurfaceData();
                blendPrimitive.createVertexData();
                Element element = new Element(mLayerSize);
                element.copyLayer(mElementList[picks[0].mElementNo]);
                element.mPrimitive = blendPrimitive;
                element.mName = mElementList[picks[0].mElementNo].mName;
                if (0 > element.mName.IndexOf("ブレンド"))
                    element.mName += "-ブレンド";
                element.mOperationNo = mOperationCount;
                element.update3DData();
                mElementList.Add(element);
            }
            mElementList[picks[0].mElementNo].mRemove = true;
            addLink(picks[0].mElementNo);
            mElementList[picks[1].mElementNo].mRemove = true;
            addLink(picks[1].mElementNo);
        }

        /// <summary>
        /// 回転体
        /// </summary>
        /// <param name="picks">ピック要素</param>
        public void revolution(List<PickData> picks)
        {
            if (picks.Count < 2)
                return;

            Primitive centerLinePrimitive = mElementList[picks[0].mElementNo].mPrimitive;
            Primitive outlinePrimitive = mElementList[picks[1].mElementNo].mPrimitive;
            //  中心線
            Line3D centerLine;
            if (centerLinePrimitive.mPrimitiveId == PrimitiveId.Line) {
                LinePrimitive linePrimitive = (LinePrimitive)centerLinePrimitive;
                centerLine = linePrimitive.mLine.toCopy();
            } else if (centerLinePrimitive.mPrimitiveId == PrimitiveId.Polyline) {
                PolylinePrimitive polylinePrimitive = (PolylinePrimitive)centerLinePrimitive;
                centerLine = new Line3D(polylinePrimitive.mPolyline.toPoint3D(0), polylinePrimitive.mPolyline.toPoint3D(1));
            } else
                return;
            //  外形線
            Polyline3D outline;
            if (outlinePrimitive.mPrimitiveId == PrimitiveId.Line) {
                LinePrimitive linePrimitive = (LinePrimitive)outlinePrimitive;
                outline = new Polyline3D(linePrimitive.mLine);
            } else if (outlinePrimitive.mPrimitiveId == PrimitiveId.Arc) {
                ArcPrimitive arcPrimitive = (ArcPrimitive)outlinePrimitive;
                outline = new Polyline3D(arcPrimitive.mArc, mArcDivideAng);
            } else if (outlinePrimitive.mPrimitiveId == PrimitiveId.Polyline) {
                PolylinePrimitive polylinePrimitive = (PolylinePrimitive)outlinePrimitive;
                outline = polylinePrimitive.mPolyline.toCopy();
            } else if (outlinePrimitive.mPrimitiveId == PrimitiveId.Polygon) {
                PolygonPrimitive polygonPrimitive = (PolygonPrimitive)outlinePrimitive;
                outline = new Polyline3D(polygonPrimitive.mPolygon);
            } else
                return;

            RevolutionPrimitive revolution = new RevolutionPrimitive(centerLine, outline, 
                mRevolutionDivideAng, false);
            if (revolution == null)
                return;
            revolution.copyProperty(outlinePrimitive);
            revolution.createSurfaceData();
            revolution.createVertexData();
            revolution.mPrimitiveId = PrimitiveId.Revolution;
            revolution.mPick = false;

            Element element = new Element(mLayerSize);
            element.mName = mElementList[picks[1].mElementNo].mName;
            if (0 > element.mName.IndexOf("回転体"))
                element.mName += "-回転体";
            element.copyLayer(mElementList[picks[1].mElementNo]);
            element.mPrimitive = revolution;
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);

            mElementList[picks[0].mElementNo].mRemove = true;
            addLink(picks[0].mElementNo);
            mElementList[picks[1].mElementNo].mRemove = true;
            addLink(picks[1].mElementNo);
        }

        /// <summary>
        /// 掃引(スィープ)
        /// </summary>
        /// <param name="picks">ピック要素</param>
        public void sweep(List<PickData> picks)
        {
            if (picks.Count < 2)
                return;

            Primitive outline1Primitive = mElementList[picks[0].mElementNo].mPrimitive;
            Primitive outline2Primitive = mElementList[picks[1].mElementNo].mPrimitive;
            //  外形線1
            Polyline3D outline1;
            if (outline1Primitive.mPrimitiveId == PrimitiveId.Polyline) {
                PolylinePrimitive polylinePrimitive = (PolylinePrimitive)outline1Primitive;
                outline1 = polylinePrimitive.mPolyline.toCopy();
            } else if (outline1Primitive.mPrimitiveId == PrimitiveId.Arc) {
                ArcPrimitive arcPrimitive = (ArcPrimitive)outline1Primitive;
                outline1 = arcPrimitive.mArc.toPolyline3D(mArcDivideAng);
            } else
                return;
            //  外形線2
            Polyline3D outline2;
            if (outline2Primitive.mPrimitiveId == PrimitiveId.Polyline) {
                PolylinePrimitive polylinePrimitive = (PolylinePrimitive)outline2Primitive;
                outline2 = polylinePrimitive.mPolyline.toCopy();
            } else if (outline2Primitive.mPrimitiveId == PrimitiveId.Arc) {
                ArcPrimitive arcPrimitive = (ArcPrimitive)outline2Primitive;
                outline2 = arcPrimitive.mArc.toPolyline3D(mArcDivideAng);
            } else
                return;
            //  Sweepデータを作成
            SweepPrimitive sweep = new SweepPrimitive(outline1, outline2, mSweepDivideAng);
            if (sweep == null)
                return;
            sweep.copyProperty(outline1Primitive);
            sweep.createSurfaceData();
            sweep.createVertexData();
            sweep.mPrimitiveId = PrimitiveId.Sweep;
            sweep.mPick = false;

            Element element = new Element(mLayerSize);
            element.mName = mElementList[picks[0].mElementNo].mName;
            if (0 > element.mName.IndexOf("掃引"))
                element.mName += "-掃引";
            element.copyLayer(mElementList[picks[0].mElementNo]);
            element.mPrimitive = sweep;
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);

            mElementList[picks[0].mElementNo].mRemove = true;
            addLink(picks[0].mElementNo);
            mElementList[picks[1].mElementNo].mRemove = true;
            addLink(picks[1].mElementNo);
        }

        /// <summary>
        /// 解除(押出や回転体を解除して元のポリゴンや線分に戻す)
        /// </summary>
        /// <param name="picks"></param>
        public void release(List<PickData> picks)
        {
            for (int i = 0; i < picks.Count; i++) {
                Element element = mElementList[picks[i].mElementNo].toCopy();
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Extrusion) {
                    //  押出解除
                    ExtrusionPrimitive extrusion = (ExtrusionPrimitive) element.mPrimitive;
                    if (extrusion.mEdgeDisp && extrusion.mLoop)
                        addPolygon(extrusion.mPolygon,extrusion.mLineColor, extrusion.mFaceColors[0], extrusion.mDivideAngle);
                    else
                        addPolyline(extrusion.mPolygon.toPolyline3D(0, extrusion.mLoop), extrusion.mLineColor, extrusion.mFaceColors[0], extrusion.mDivideAngle);
                } else if (element.mPrimitive.mPrimitiveId == PrimitiveId.Blend) {
                    //  ブレンド解除
                    BlendPrimitive blend = (BlendPrimitive)element.mPrimitive;
                    addPolygon(blend.mPolygon1, blend.mLineColor, blend.mFaceColors[0], blend.mDivideAngle);
                    addPolygon(blend.mPolygon2, blend.mLineColor, blend.mFaceColors[0], blend.mDivideAngle);
                } else if (element.mPrimitive.mPrimitiveId == PrimitiveId.BlendPolyline) {
                    //  ブレンドポリライン解除
                    BlendPolylinePrimitive blend = (BlendPolylinePrimitive)element.mPrimitive;
                    addPolyline(blend.mPolyline1, blend.mLineColor, blend.mFaceColors[0], blend.mDivideAngle);
                    addPolyline(blend.mPolyline2, blend.mLineColor, blend.mFaceColors[0], blend.mDivideAngle);
                } else if (element.mPrimitive.mPrimitiveId == PrimitiveId.Revolution) {
                    //  回転体解除
                    RevolutionPrimitive revolution = (RevolutionPrimitive) element.mPrimitive;
                    addPolyline(revolution.mCenterLine.toPoint3D(), revolution.mLineColor, revolution.mFaceColors[0], revolution.mDivideAngle);
                    addPolyline(revolution.mOutLine.toPoint3D(), revolution.mLineColor, revolution.mFaceColors[0], revolution.mDivideAngle);
                } else if (element.mPrimitive.mPrimitiveId == PrimitiveId.Sweep) {
                    //  掃引解除
                    SweepPrimitive sweep = (SweepPrimitive) element.mPrimitive;
                    addPolyline(sweep.mOutLine1.toPoint3D(), sweep.mLineColor, sweep.mFaceColors[0]);
                    addPolyline(sweep.mOutLine2.toPoint3D(), sweep.mLineColor, sweep.mFaceColors[0]);
                } else {
                    continue;
                }
                mElementList[picks[i].mElementNo].mRemove = true;
                addLink(picks[i].mElementNo);
            }
        }

        /// <summary>
        /// Elementの属性変更
        /// </summary>
        /// <param name="picks">ピック要素</param>
        public void changeProperty(List<PickData> picks)
        {
            for (int i = 0; i < picks.Count; i++) {
                Element element = mElementList[picks[i].mElementNo].toCopy();
                PropertyDlg dlg = new PropertyDlg();
                dlg.Owner = mMainWindow;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlg.Title = "属性変更 " + element.mPrimitive.mPrimitiveId;
                dlg.mName = element.mName;
                dlg.mLineColor = element.mPrimitive.mLineColor;
                dlg.mLineFont = element.mPrimitive.mLineType;
                dlg.mFaceColor = element.mPrimitive.mFaceColors[0];
                dlg.mReverse = element.mPrimitive.mReverse;
                dlg.mDisp3D = element.mDisp3D;
                dlg.mEdgeDisp = element.mPrimitive.mEdgeDisp;
                dlg.mOutlineDisp = element.mPrimitive.mOutlineDisp;
                dlg.mBothShading = element.mBothShading;
                dlg.mBothShadingEnable = false;
                dlg.mDivideAng = ylib.R2D(element.mPrimitive.mDivideAngle);
                dlg.mChkList = mLayer.getLayerChkList(element.mLayerBit);
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Arc) {
                    dlg.mArcOn = true;
                    ArcPrimitive arc = (ArcPrimitive)element.mPrimitive;
                    dlg.mArcRadius = arc.mArc.mR;
                    dlg.mArcStartAngle = ylib.R2D(arc.mArc.mSa);
                    dlg.mArcEndAngle = ylib.R2D(arc.mArc.mEa);
                    dlg.mDivideAngOn = true;
                }
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Polyline) {
                    dlg.mDivideAngOn = true;
                }
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Polygon) {
                    dlg.mReverseOn = true;
                    dlg.mDivideAngOn = true;
                }
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Extrusion) {
                    dlg.mLineFontOn = false;
                    dlg.mReverseOn = true;
                    dlg.mDivideAngOn = true;
                    dlg.mEdgeDispEnable = true;
                    dlg.mOutlineDispEnable = true;
                }
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Blend) {
                    dlg.mLineFontOn = false;
                    dlg.mReverseOn = true;
                    dlg.mDivideAngOn = true;
                    dlg.mEdgeDispEnable = true;
                    dlg.mOutlineDispEnable = true;
                }
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.BlendPolyline) {
                    dlg.mLineFontOn = false;
                    dlg.mReverseOn = true;
                    dlg.mDivideAngOn = true;
                    dlg.mEdgeDispEnable = false;
                    dlg.mOutlineDispEnable = true;
                }
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Revolution) {
                    RevolutionPrimitive revolution = (RevolutionPrimitive)element.mPrimitive;
                    dlg.mArcOn = true;
                    dlg.mArcRadiusOn = false;
                    dlg.mArcStartAngle = ylib.R2D(revolution.mSa);
                    dlg.mArcEndAngle = ylib.R2D(revolution.mEa);
                    dlg.mLineFontOn = false;
                    dlg.mReverseOn = true;
                    dlg.mDivideAngOn = true;
                    dlg.mEdgeDispEnable = true;
                    dlg.mOutlineDispEnable = true;
                }
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Sweep) {
                    SweepPrimitive sweep = (SweepPrimitive)element.mPrimitive;
                    dlg.mDivideAngOn = true;
                    dlg.mArcOn = true;
                    dlg.mArcRadiusOn = false;
                    dlg.mArcStartAngle = ylib.R2D(sweep.mSa);
                    dlg.mArcEndAngle = ylib.R2D(sweep.mEa);
                    dlg.mLineFontOn = false;
                    dlg.mReverseOn = true;
                    dlg.mDivideAngOn = true;
                    dlg.mEdgeDispEnable = true;
                    dlg.mOutlineDispEnable = true;
                }

                if (dlg.ShowDialog() == true) {
                    element.mName = dlg.mName;
                    element.mPrimitive.mLineColor = dlg.mLineColor;
                    element.mPrimitive.mLineType = dlg.mLineFont;
                    element.mPrimitive.mFaceColors[0] = dlg.mFaceColor;
                    element.mDisp3D = dlg.mDisp3D;
                    element.mPrimitive.mOutlineDisp = dlg.mOutlineDisp;
                    element.mBothShading = dlg.mBothShading;
                    element.mLayerBit = mLayer.setLayerChkList(element.mLayerBit, dlg.mChkList);
                    element.mOperationNo = mOperationCount;
                    if (element.mPrimitive.mPrimitiveId == PrimitiveId.Arc) {
                        ArcPrimitive arc = (ArcPrimitive)element.mPrimitive;
                        arc.mArc.mR = dlg.mArcRadius;
                        arc.mArc.mSa = ylib.D2R(dlg.mArcStartAngle);
                        arc.mArc.mEa = ylib.D2R(dlg.mArcEndAngle);
                    }
                    if (element.mPrimitive.mPrimitiveId == PrimitiveId.Revolution) {
                        RevolutionPrimitive revolution = (RevolutionPrimitive)element.mPrimitive;
                        revolution.mSa = ylib.D2R(dlg.mArcStartAngle);
                        revolution.mEa = ylib.D2R(dlg.mArcEndAngle);
                    }
                    if (element.mPrimitive.mPrimitiveId == PrimitiveId.Sweep) {
                        SweepPrimitive sweep = (SweepPrimitive)element.mPrimitive;
                        sweep.mSa = ylib.D2R(dlg.mArcStartAngle);
                        sweep.mEa = ylib.D2R(dlg.mArcEndAngle);
                    }
                    element.mPrimitive.mEdgeDisp = dlg.mEdgeDisp;
                    element.mPrimitive.mReverse = dlg.mReverse;
                    element.mPrimitive.mDivideAngle = ylib.D2R(dlg.mDivideAng);
                    element.mPrimitive.createSurfaceData();
                    element.mPrimitive.createVertexData();
                    element.update3DData();
                    mElementList.Add(element);
                    mElementList[picks[i].mElementNo].mRemove = true;
                    addLink(picks[i].mElementNo);
                    if (mMainWindow.mCommandOpe.mLayerChkListDlg != null && mMainWindow.mCommandOpe.mLayerChkListDlg.IsVisible)
                        mMainWindow.mCommandOpe.setDispLayer();
                }
            }
        }

        /// <summary>
        /// 属性の一括変更
        /// </summary>
        /// <param name="picks">ピック要素</param>
        public void changePropertyAll(List<PickData> picks)
        {
            if (picks.Count == 0) return;
            PropertyDlg dlg = new PropertyDlg();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = "一括属性変更 ";
            dlg.mPropertyAll = true;
            dlg.mChkList = mLayer.getLayerChkList(true);
            dlg.mOutlineDispEnable = true;
            if (dlg.ShowDialog() != true) return;
            for (int i = 0; i < picks.Count; i++) {
                Element element = mElementList[picks[i].mElementNo].toCopy();
                if (dlg.mNameEnable)
                    element.mName = dlg.mName;
                if (dlg.mLineColoeEnable)
                    element.mPrimitive.mLineColor = dlg.mLineColor;
                if (dlg.mLineFontEnable)
                    element.mPrimitive.mLineType = dlg.mLineFont;
                if (dlg.mFaceColorEnable)
                    element.mPrimitive.mFaceColors[0] = dlg.mFaceColor;
                if (dlg.mDisp3DEnable)
                    element.mDisp3D = dlg.mDisp3D;
                if (dlg.mEdgeDispEnable)
                    element.mPrimitive.mEdgeDisp = dlg.mEdgeDisp;
                if (dlg.mBothShadingEnable)
                    element.mBothShading = dlg.mBothShading;
                if (dlg.mEdgeDispEnable)
                    element.mPrimitive.mEdgeDisp = dlg.mEdgeDisp;
                if (dlg.mOutlineDispEnable &&
                    (element.mPrimitive.mPrimitiveId == PrimitiveId.Extrusion ||
                    element.mPrimitive.mPrimitiveId == PrimitiveId.Blend ||
                    element.mPrimitive.mPrimitiveId == PrimitiveId.Revolution ||
                    element.mPrimitive.mPrimitiveId == PrimitiveId.Sweep))
                    element.mPrimitive.mOutlineDisp = dlg.mOutlineDisp;
                if (dlg.mCkkListEnable)
                    element.mLayerBit = mLayer.setLayerChkList(element.mLayerBit, dlg.mChkList, dlg.mCkkListAdd == false);
                if (dlg.mDivideAngEnable)
                    element.mPrimitive.mDivideAngle = ylib.D2R(dlg.mDivideAng);
                element.mOperationNo = mOperationCount;
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Arc) {
                    ArcPrimitive arc = (ArcPrimitive)element.mPrimitive;
                    if (dlg.mArcRadiusEnable)
                        arc.mArc.mR = dlg.mArcRadius;
                    if (dlg.mArcStartAngleEnable)
                        arc.mArc.mSa = ylib.D2R(dlg.mArcStartAngle);
                    if (dlg.mArcEndAngleEnable)
                        arc.mArc.mEa = ylib.D2R(dlg.mArcEndAngle);
                }
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Revolution) {
                    RevolutionPrimitive revolution = (RevolutionPrimitive)element.mPrimitive;
                    if (dlg.mArcStartAngleEnable)
                        revolution.mSa = ylib.D2R(dlg.mArcStartAngle);
                    if (dlg.mArcEndAngleEnable)
                        revolution.mEa = ylib.D2R(dlg.mArcEndAngle);
                }
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Sweep) {
                    SweepPrimitive sweep = (SweepPrimitive)element.mPrimitive;
                    if (dlg.mArcStartAngleEnable)
                        sweep.mSa = ylib.D2R(dlg.mArcStartAngle);
                    if (dlg.mArcEndAngleEnable)
                        sweep.mEa = ylib.D2R(dlg.mArcEndAngle);
                }
                if (dlg.mReverse && dlg.mReverseEnable) {
                    if (element.mPrimitive.mPrimitiveId == PrimitiveId.Polygon) {
                        PolygonPrimitive polygon = (PolygonPrimitive)element.mPrimitive;
                        polygon.mPolygon.mPolygon.Reverse();
                    } else if (element.mPrimitive.mPrimitiveId == PrimitiveId.Revolution) {
                        RevolutionPrimitive revolution = (RevolutionPrimitive)element.mPrimitive;
                        revolution.mOutLine.mPolyline.Reverse();
                    }
                }
                element.mPrimitive.createSurfaceData();
                element.mPrimitive.createVertexData();
                element.update3DData();
                mElementList.Add(element);
                mElementList[picks[i].mElementNo].mRemove = true;
                addLink(picks[i].mElementNo);
                if (dlg.mCkkListEnable && 
                    mMainWindow.mCommandOpe.mLayerChkListDlg != null && 
                    mMainWindow.mCommandOpe.mLayerChkListDlg.IsVisible)
                    mMainWindow.mCommandOpe.setDispLayer();
            }
        }

        /// <summary>
        /// ピックした要素データをクリップボードにコピー
        /// </summary>
        /// <param name="picks"></param>
        public void copyElement(List<PickData> picks)
        {
            if (picks.Count == 0)
                return;
            List<string[]> listData = new List<string[]>();
            List<string> listLayer = new List<string>();
            //  要素データの取得
            Box3D area = mElementList[picks[0].mElementNo].mArea.toCopy();
            for (int i = 0; i < picks.Count; i++) {
                Element element = mElementList[picks[i].mElementNo].toCopy();
                if (element.isDraw(mLayer)) {
                    listData.AddRange(element.toDataList());
                    area.extension(element.mArea);
                    listLayer.AddRange(mLayer.getLayerNameList(element.mLayerBit));
                }
            }
            string buf = mMainWindow.mAppName + "\n";
            //  領域
            buf += "area," + area.ToString() + "\n";
            buf = buf.Replace(") (", ",");
            buf = buf.Replace("(", "");
            buf = buf.Replace(")", "");
            //  使用レイヤ
            listLayer.Distinct();
            buf += "layer,";
            foreach (string layerName in listLayer) {
                buf += $"{mLayer.getLayerNo(layerName)},{layerName},"; 
            }
            buf = buf.TrimEnd(',') + "\n";
            //  プリミティブデータ
            foreach (string[] str in listData) {
                buf += ylib.arrayStr2CsvData(str) + "\n";
            }
            //  Clipboardにコピー
            System.Windows.DataObject data = new System.Windows.DataObject(System.Windows.DataFormats.Text, buf);
            System.Windows.Clipboard.SetDataObject(data, true);
        }

        /// <summary>
        /// クリップボードの要素データを取得する
        /// </summary>
        public void getPasteElement()
        {
            string data = System.Windows.Clipboard.GetText();
            mCopyElementList = new List<Element>();
            List<int[]> replaceLayer = new List<int[]>();
            if (0 < data.Length) {
                List<string[]> dataList = new List<string[]>();
                string[] strList = data.Split(new char[] { '\n' });
                for (int i = 0; i < strList.Length; i++)
                    dataList.Add(ylib.csvData2ArrayStr(strList[i]));
                if (1 < dataList.Count && dataList[0][0] == "CadApp") {
                    getCadAppData(dataList);
                    return;
                } else if (1 > dataList.Count || dataList[0][0] != mMainWindow.mAppName)
                    return;
                if (0 < dataList.Count) {
                    int sp = 1;
                    while (sp < dataList.Count - 1) {
                        string[] strArray = dataList[sp++];
                        if (4 < strArray.Length && strArray[0] == "area") {
                            mCopyArea = new Box3D($"{strArray[1]},{strArray[2]},{strArray[3]},{strArray[4]},{strArray[5]},{strArray[6]}");
                            mCopyArea.normalize();
                        } else if (0 < strArray.Length && strArray[0] == "layer") {
                            for (int i = 1; i < strArray.Length; i += 2) {
                                mLayer.add(strArray[i + 1]);
                                int oldLayNo = int.Parse(strArray[i]);
                                int newLayNo = mLayer.getLayerNo(strArray[i + 1]);
                                int[] layNo = new int[] { oldLayNo, newLayNo };
                                replaceLayer.Add(layNo);
                            }
                        } else {
                            sp--;
                            break;
                        }
                    }
                    Element element;
                    while (sp < dataList.Count - 1) {
                        string[] buf = dataList[sp++];
                        if (buf[0] == "Element") {
                            element = new Element(mLayerSize);
                            sp = element.setDataList(dataList, sp, mWireFrame, mSurfaceVertex);
                            if (element.mPrimitive != null && 0 < element.mPrimitive.mSurfaceDataList.Count) {
                                element.mLayerBit = mLayer.replaceOn(element.mLayerBit, replaceLayer);
                                //mLayer.bitOnAll(element.mLayerBit, true);
                                mCopyElementList.Add(element);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// CadAppの要素コピーデータの取り込み
        /// </summary>
        /// <param name="dataList">要素コピーデータ</param>
        public void getCadAppData(List<string[]> dataList)
        {
            mCopyElementList = new List<Element>();
            List<int[]> replaceLayer = new List<int[]>();
            if (1 < dataList.Count) {
                int sp = 1;
                string[] areaStr = dataList[sp++];
                if (4 < dataList[1].Length && dataList[1][0] == "area") {
                    Box b = new Box($"{areaStr[1]},{areaStr[2]},{areaStr[3]},{areaStr[4]}");
                    mCopyArea = new Box3D(b, mFace);
                }
                Element element;
                while (sp < dataList.Count) {
                    string[] property = dataList[sp++];
                    if (property.Length <= 4)
                        continue;
                    string[] data = dataList[sp++];
                    element = getCadAppProperty(property, data, mFace);
                    if (element != null && element.mPrimitive != null)
                        mCopyElementList.Add(element);
                }
            }
        }

        /// <summary>
        /// CadAppの要素データの取り込み
        /// </summary>
        /// <param name="property">属性データ</param>
        /// <param name="data">幾何データ</param>
        /// <param name="face">2D平面</param>
        /// <returns>Element</returns>
        public Element getCadAppProperty(string[] property, string[] data, FACE3D face)
        {
            Element element = new Element(mLayerSize);
            switch (property[0]) {
                case "Point":
                    PointD p2d = new PointD(ylib.doubleParse(data[0]), ylib.doubleParse(data[1]));
                    element.mPrimitive = new PointPrimitive(new Point3D(p2d, face));
                    break;
                case "Line":
                    PointD ps = new PointD(ylib.doubleParse(data[0]), ylib.doubleParse(data[1]));
                    PointD pe = new PointD(ylib.doubleParse(data[2]), ylib.doubleParse(data[3]));
                    element.mPrimitive = new LinePrimitive(new Line3D(ps, pe, face));
                    break;
                case "Arc":
                    ArcD arc = new ArcD();
                    arc.mCp = new PointD(ylib.doubleParse(data[3]), ylib.doubleParse(data[4]));
                    arc.mR = ylib.doubleParse(data[5]);
                    arc.mSa = ylib.doubleParse(data[6]);
                    arc.mEa = ylib.doubleParse(data[7]);
                    element.mPrimitive = new ArcPrimitive(new Arc3D(arc, face));
                    break;
                case "Polyline":
                    PolylineD polyline = new PolylineD();
                    for (int i = 0; i < data.Length - 1; i += 2) {
                        PointD p = new PointD(ylib.doubleParse(data[i]), ylib.doubleParse(data[i + 1]));
                        polyline.Add(p);
                    }
                    polyline.squeeze();
                    element.mPrimitive = new PolylinePrimitive(new Polyline3D(polyline, face));
                    break;
                case "Polygon":
                    PolygonD polygon = new PolygonD();
                    for (int i = 3; i < data.Length - 1; i += 2) {
                        PointD p = new PointD(ylib.doubleParse(data[i]), ylib.doubleParse(data[i + 1]));
                        polygon.Add(p);
                    }
                    polygon.squeeze();
                    element.mPrimitive = new PolygonPrimitive(new Polygon3D(polygon.toPointList(), face));
                    break;
                default:
                    element = null;
                    break;
            }
            if (element != null) {
                element.mPrimitive.mLineColor = ylib.getBrsh(property[1].Trim());
                element.mPrimitive.mFaceColors[0] = ylib.getBrsh(property[1].Trim());
                element.mPrimitive.mLineThickness = ylib.doubleParse(property[2].Trim());
                element.mPrimitive.mLineType = ylib.intParse(property[3].Trim());
                element.mPrimitive.createSurfaceData();
                element.mPrimitive.createVertexData();
            }
            return element;
        }

        /// <summary>
        /// クリップボードから取得した要素データを貼り付ける
        /// </summary>
        /// <param name="loc">配置座標</param>
        public void pasteElement(PointD loc)
        {
            if (mCopyArea == null)
                return;
            Point3D vec = new Point3D(loc, mFace) - mCopyArea.mMin;
            for (int i = 0; i < mCopyElementList.Count; i++) {
                Element element = mCopyElementList[i];
                element.mPrimitive.translate(vec, new PointD(), mFace);
                element.mOperationNo = mOperationCount;
                element.mPrimitive.createSurfaceData();
                element.mPrimitive.createVertexData();
                element.update3DData();
                mElementList.Add(element);
            }
        }

        /// <summary>
        /// 点プリミティブの作成
        /// </summary>
        /// <param name="p">座標</param>
        /// <returns>プリミティブ</returns>
        public Primitive createPoint(Point3D p)
        {
            PointPrimitive point = new PointPrimitive(p);
            point.mLineColor = mPrimitiveBrush;
            point.mFaceColors[0] = mPrimitiveBrush;
            point.mSurfaceVertex = mSurfaceVertex;
            point.mWireFrame = mWireFrame;
            point.createSurfaceData();
            point.createVertexData();
            return point;
        }

        /// <summary>
        /// 線分プリミティブの作成
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <returns>プリミティブ</returns>
        public Primitive createLine(PointD sp, PointD ep)
        {
            return createLine(new Line3D(sp, ep, mFace));
        }

        /// <summary>
        /// 線分プリミティブの作成
        /// </summary>
        /// <param name="l"></param>
        /// <returns>プリミティブ</returns>
        public Primitive createLine(Line3D l)
        {
            LinePrimitive line = new LinePrimitive(l);
            line.mLineColor = mPrimitiveBrush;
            line.mFaceColors[0] = mPrimitiveBrush;
            line.mSurfaceVertex = mSurfaceVertex;
            line.mWireFrame = mWireFrame;
            line.createSurfaceData();
            line.createVertexData();
            return line;
        }

        /// <summary>
        /// 円プリミティブの作成
        /// </summary>
        /// <param name="cp">中心</param>
        /// <param name="ep">円周上位置</param>
        /// <returns></returns>
        public Primitive createCircle(PointD cp, PointD ep)
        {
            Arc3D arc3D = new Arc3D(new ArcD(cp, ep, Math.PI * 2), mFace);
            return createArc(arc3D);
        }

        /// <summary>
        /// 円弧プリミティブの作成
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="mp">中間点</param>
        /// <param name="ep">終点</param>
        /// <returns>プリミティブ</returns>
        public Primitive createArc(PointD sp, PointD mp, PointD ep)
        {
            Arc3D arc3D = new Arc3D(new ArcD(sp, mp, ep), mFace);
            return createArc(arc3D);
        }

        /// <summary>
        /// 円プリミティブの作成
        /// </summary>
        /// <param name="arc3D">円データ</param>
        /// <returns>プリミティブ</returns>
        public Primitive createArc(Arc3D arc3D)
        {
            ArcPrimitive arc = new ArcPrimitive(arc3D, mArcDivideAng);
            arc.mLineColor = mPrimitiveBrush;
            arc.mFaceColors[0] = mPrimitiveBrush;
            arc.mSurfaceVertex = mSurfaceVertex;
            arc.mWireFrame = mWireFrame;
            arc.createSurfaceData();
            arc.createVertexData();
            return arc;
        }

        /// <summary>
        /// ポリラインプリミティブの作成
        /// </summary>
        /// <param name="plist">2D座標点リスト</param>
        /// <returns>プリミティブ</returns>
        public Primitive createPolyline(List<PointD> plist)
        {
            return createPolyline(new Polyline3D(plist, mFace));
        }

        /// <summary>
        /// ポリラインプリミティブの作成
        /// </summary>
        /// <param name="polyline">ポリライン</param>
        /// <returns>プリミティブ</returns>
        public Primitive createPolyline(Polyline3D polyline)
        {
            PolylinePrimitive polylinePrimitive = new PolylinePrimitive(polyline);
            polylinePrimitive.mLineColor = mPrimitiveBrush;
            polylinePrimitive.mFaceColors[0] = mPrimitiveBrush;
            polylinePrimitive.mSurfaceVertex = mSurfaceVertex;
            polylinePrimitive.mWireFrame = mWireFrame;
            polylinePrimitive.createSurfaceData();
            polylinePrimitive.createVertexData();
            return polylinePrimitive;
        }

        /// <summary>
        /// ポリゴンプリミティブの作成
        /// </summary>
        /// <param name="plist">2D座標点リスト</param>
        /// <returns>プリミティブ</returns>
        public Primitive createPolygon(List<PointD> plist)
        {
            return createPolygon(new Polygon3D(plist, mFace));
        }

        /// <summary>
        /// ポリゴンプリミティブの作成
        /// </summary>
        /// <param name="polygon">3D座標リスト</param>
        /// <returns>ポリゴンプリミティブ</returns>
        public Primitive createPolygon(Polygon3D polygon)
        {
            PolygonPrimitive polygonPrimitive = new PolygonPrimitive(polygon, mFace);
            polygonPrimitive.mLineColor = mPrimitiveBrush;
            polygonPrimitive.mFaceColors[0] = mPrimitiveBrush;
            polygonPrimitive.mSurfaceVertex = mSurfaceVertex;
            polygonPrimitive.mWireFrame = mWireFrame;
            polygonPrimitive.createSurfaceData();
            polygonPrimitive.createVertexData();
            return polygonPrimitive;
        }

        /// <summary>
        /// 押出プリミティブ作成
        /// </summary>
        /// <param name="primitive">元となるプリミティブ</param>
        /// <param name="v">押出ベクトル</param>
        /// <returns></returns>
        public ExtrusionPrimitive createExtrusion(Primitive primitive, Point3D v)
        {
            ExtrusionPrimitive extrusion = new();
            if (primitive.mPrimitiveId == PrimitiveId.Line ||
                primitive.mPrimitiveId == PrimitiveId.Arc ||
                primitive.mPrimitiveId == PrimitiveId.Polyline) {
                extrusion = new ExtrusionPrimitive(new Polygon3D(primitive.mSurfaceDataList[0].mVertexList), v, false);
            } else if (primitive.mPrimitiveId == PrimitiveId.Polygon) {
                PolygonPrimitive polygon = (PolygonPrimitive)primitive;
                extrusion = new ExtrusionPrimitive(polygon.mPolygon, v, true);
            } else {
                extrusion = new ExtrusionPrimitive(new Polygon3D(primitive.mSurfaceDataList[0].mVertexList), v, false);
            }
            extrusion.copyProperty(primitive);
            extrusion.createSurfaceData();
            extrusion.createVertexData();
            extrusion.mPrimitiveId = PrimitiveId.Extrusion;
            extrusion.mPick = false;
            return extrusion;
        }

        /// <summary>
        /// ポリゴンの分割
        /// </summary>
        /// <param name="element">ピック要素</param>
        /// <param name="locPos">分割点</param>
        /// <returns>結果</returns>
        public bool dividePolygon(Element element, PointD locPos)
        {
            PolygonPrimitive polygonPrimitive = (PolygonPrimitive)element.mPrimitive;
            Polyline3D polyline = polygonPrimitive.mPolygon.divide(new Point3D(locPos, mFace));
            if (polyline == null)
                return false;
            Element ele = new Element(mLayerSize);
            ele.mName = element.mName;
            ele.mPrimitive = new PolylinePrimitive(polyline);
            ele.mPrimitive.copyProperty(polygonPrimitive);
            ele.mPrimitive.createSurfaceData();
            ele.mPrimitive.createVertexData();
            ele.mPrimitive.mPrimitiveId = PrimitiveId.Polyline;
            ele.copyLayer(element);
            ele.mOperationNo = mOperationCount;
            ele.update3DData();
            mElementList.Add(ele);
            return true;
        }

        /// <summary>
        /// ポリライン要素を分割
        /// </summary>
        /// <param name="element">ピック要素</param>
        /// <param name="locPos">分割点</param>
        /// <returns>結果</returns>
        public bool dividePolyline(Element element, PointD locPos)
        {
            PolylinePrimitive polylinePrimitive = (PolylinePrimitive)element.mPrimitive;
            List<Polyline3D> polylines = polylinePrimitive.mPolyline.divide(locPos, mFace);
            if (1 < polylines.Count) {
                Element ele1 = new Element(mLayerSize);
                ele1.mName = element.mName + "_1";
                ele1.mPrimitive = new PolylinePrimitive(polylines[1]);
                ele1.mPrimitive.copyProperty(polylinePrimitive);
                ele1.mPrimitive.createSurfaceData();
                ele1.mPrimitive.createVertexData();
                ele1.copyLayer(element);
                ele1.mOperationNo = mOperationCount;
                ele1.update3DData();
                mElementList.Add(ele1);
            }
            if (0 < polylines.Count) {
                Element ele1 = new Element(mLayerSize);
                ele1.mName = element.mName + "_2";
                ele1.mPrimitive = new PolylinePrimitive(polylines[0]);
                ele1.mPrimitive.copyProperty(polylinePrimitive);
                ele1.mPrimitive.createSurfaceData();
                ele1.mPrimitive.createVertexData();
                ele1.copyLayer(element);
                ele1.mOperationNo = mOperationCount;
                ele1.update3DData();
                mElementList.Add(ele1);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 線分要素の分割
        /// </summary>
        /// <param name="element">ピック要素</param>
        /// <param name="locPos">分割点</param>
        /// <returns>結果</returns>
        public bool divideLine(Element element, PointD locPos)
        {
            LinePrimitive linePrimitive = (LinePrimitive)element.mPrimitive;
            List<Line3D> lines = linePrimitive.mLine.divide(locPos, mFace);
            if (1 < lines.Count) {
                Element ele1 = new Element(mLayerSize);
                ele1.mName = element.mName + "_1";
                ele1.mPrimitive = new LinePrimitive(lines[1]);
                ele1.mPrimitive.copyProperty(linePrimitive);
                ele1.mPrimitive.createSurfaceData();
                ele1.mPrimitive.createVertexData();
                ele1.copyLayer(element);
                ele1.mOperationNo = mOperationCount;
                ele1.update3DData();
                mElementList.Add(ele1);
            }
            if (0 < lines.Count) {
                Element ele1 = new Element(mLayerSize);
                ele1.mName = element.mName + "_2";
                ele1.mPrimitive = new LinePrimitive(lines[0]);
                ele1.mPrimitive.copyProperty(linePrimitive);
                ele1.mPrimitive.createSurfaceData();
                ele1.mPrimitive.createVertexData();
                ele1.copyLayer(element);
                ele1.mOperationNo = mOperationCount;
                ele1.update3DData();
                mElementList.Add(ele1);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 円弧要素を分割する
        /// </summary>
        /// <param name="element">ピック要素</param>
        /// <param name="locPos">分割点</param>
        /// <returns>結果</returns>
        public bool divideArc(Element element, PointD locPos)
        {
            ArcPrimitive arcPrimitive = (ArcPrimitive)element.mPrimitive;
            double divideAng = arcPrimitive.mDivideAngle;
            List<Arc3D> arcs = arcPrimitive.mArc.divide(locPos, mFace);
            if (1 < arcs.Count) {
                Element ele1 = new Element(mLayerSize);
                ele1.mName = element.mName + "_1";
                ele1.mPrimitive = new ArcPrimitive(arcs[1], divideAng);
                ele1.mPrimitive.copyProperty(arcPrimitive);
                ele1.mPrimitive.createSurfaceData();
                ele1.mPrimitive.createVertexData();
                ele1.copyLayer(element);
                ele1.mOperationNo = mOperationCount;
                ele1.update3DData();
                mElementList.Add(ele1);
            }
            if (0 < arcs.Count) {
                Element ele1 = new Element(mLayerSize);
                ele1.mName = element.mName + "_2";
                ele1.mPrimitive = new ArcPrimitive(arcs[0], divideAng);
                ele1.mPrimitive.copyProperty(arcPrimitive);
                ele1.mPrimitive.createSurfaceData();
                ele1.mPrimitive.createVertexData();
                ele1.copyLayer(element);
                ele1.mOperationNo = mOperationCount;
                ele1.update3DData();
                mElementList.Add(ele1);
                return true;
            }
            return false;
        }

        /// <summary>
        /// フィレットの作成(ポリラインまたはポリゴンの頂点)
        /// </summary>
        /// <param name="element">ピック要素</param>
        /// <param name="pickPos">ピック位置</param>
        /// <returns></returns>
        public bool fillet(Element element, PointD pickPos)
        {
            if (element.mPrimitive.mPrimitiveId == PrimitiveId.Polyline) {
                PolylinePrimitive polylinePrimitive = (PolylinePrimitive)element.mPrimitive;
                PolylineD polyline = new PolylineD(polylinePrimitive.mPolyline.mPolyline);
                Point3D pickPos3D = new Point3D(pickPos, mFace);
                PointD pos = pickPos3D.toPointD(polylinePrimitive.mPolyline.mCp, polylinePrimitive.mPolyline.mU, polylinePrimitive.mPolyline.mV);
                polyline.fillet(mFilletSize, pos);
                polylinePrimitive.mPolyline.mPolyline = polyline.toPointList();
                polylinePrimitive.createSurfaceData();
                polylinePrimitive.createVertexData();
            } else if (element.mPrimitive.mPrimitiveId == PrimitiveId.Polygon) {
                PolygonPrimitive polygpnPrimitive = (PolygonPrimitive)element.mPrimitive;
                PolygonD polygon = new PolygonD(polygpnPrimitive.mPolygon.mPolygon);
                Point3D pickPos3D = new Point3D(pickPos, mFace);
                PointD pos = pickPos3D.toPointD(polygpnPrimitive.mPolygon.mCp, polygpnPrimitive.mPolygon.mU, polygpnPrimitive.mPolygon.mV);
                polygon.fillet(mFilletSize, pos);
                polygpnPrimitive.mPolygon.mPolygon = polygon.toPointList();
                polygpnPrimitive.createSurfaceData();
                polygpnPrimitive.createVertexData();
            } else
                return false;

            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
            return true;
        }


        /// <summary>
        /// 計測
        /// </summary>
        /// <param name="locList">ロケイト</param>
        public void measure(List<PointD> locList)
        {
            if (locList.Count == 2) {
                string buf = "距離 : " + ylib.double2StrZeroSup(locList[0].length(locList[1]), "F8");
                ylib.messageBox(mMainWindow, buf, "", "計測");
            } else if (locList.Count == 3) {
                string buf = "角度 : " + ylib.double2StrZeroSup(ylib.R2D(locList[0].angle(locList[1], locList[2])), "F8");
                ylib.messageBox(mMainWindow, buf, "", "計測");
            }
        }

        /// <summary>
        /// 計測
        /// </summary>
        /// <param name="picks">ピック要素</param>
        public void measure(List<PickData> picks)
        {
            if (picks.Count == 1) {
                Line3D l0 = mElementList[picks[0].mElementNo].mPrimitive.getLine(picks[0].mPos, mFace);
                string buf =　"長さ : " + l0.length().ToString("F3");
                ylib.messageBox(mMainWindow, buf, "要素番号 " + picks[0].mElementNo.ToString(), "計測");
            } else if (picks.Count ==2) {
                LineD l0 = mElementList[picks[0].mElementNo].mPrimitive.getLine(picks[0].mPos, mFace).toLineD(mFace);
                LineD l1 = mElementList[picks[1].mElementNo].mPrimitive.getLine(picks[1].mPos, mFace).toLineD(mFace);
                double dis = l0.distance(l1);
                double ang = l0.angle(l1);
                string buf = "";
                if (0 <= dis)
                    buf += "距離 : " + ylib.double2StrZeroSup(dis, "F8");
                if (0 <= ang)
                    buf += (0 <= dis ? "\n" : "") + "角度 : " + ylib.double2StrZeroSup(ylib.R2D(ang), "F8");
                ylib.messageBox(mMainWindow, buf, "距離・角度測定");
            }
        }

        /// <summary>
        /// 要素情報表示
        /// </summary>
        /// <param name="picks"></param>
        public void info(List<PickData> picks)
        {
            for (int i = 0; i < picks.Count; i++) {
                string buf = mElementList[picks[i].mElementNo].propertyInfo();
                buf += " レイヤ:[" + string.Join(",", mLayer.getLayerNameList(mElementList[picks[i].mElementNo].mLayerBit)) + "]";
                buf += "\n" + mElementList[picks[i].mElementNo].mPrimitive.dataSummary("F2");
                buf += "\n" + mElementList[picks[i].mElementNo].dataInfo();
                ylib.messageBox(mMainWindow, buf,
                    "要素番号 " + picks[i].mElementNo.ToString(), "要素情報");
            }
        }

        /// <summary>
        /// ピックした要素を削除
        /// </summary>
        /// <param name="picks"></param>
        public void remove(List<PickData> picks)
        {
            for (int i = 0; i < picks.Count; i++) {
                mElementList[picks[i].mElementNo].mRemove = true;
                addLink(picks[i].mElementNo);
            }
        }

        /// <summary>
        /// アンドゥ処理
        /// </summary>
        public void undo()
        {
            if (0 < mElementList.Count) {
                int entNo = mElementList.Count - 1;
                int opeNo = mElementList[entNo].mOperationNo;
                while (0 <= entNo && 0 <= opeNo && opeNo == mElementList[entNo].mOperationNo) {
                    if (0 <= mElementList[entNo].mLinkNo) {
                        mElementList[mElementList[entNo].mLinkNo].mRemove = false;
                    }
                    mElementList.RemoveAt(entNo);
                    entNo--;
                }
            }
        }

        /// <summary>
        /// データリストの更新(作図領域を求める)
        /// </summary>
        public void updateData()
        {
            if (mElementList == null || mElementList.Count == 0) return;
            int n = 0;
            mArea = new Box3D();
            for (int i = 0; i < mElementList.Count; i++) {
                if (!mElementList[i].mRemove && 0 > mElementList[i].mLinkNo) {
                    mArea = mElementList[i].mArea.toCopy();
                    n = i + 1;
                    break;
                }
            }
            for (int j = n; j < mElementList.Count; j++) {
                if (!mElementList[j].mRemove && mElementList[j].mPrimitive != null &&
                    0 > mElementList[j].mLinkNo)
                    mArea.extension(mElementList[j].mArea);
            }
        }

        /// <summary>
        /// 表示データ再作成
        /// </summary>
        public void reCreate()
        {
            if (mElementList == null || mElementList.Count == 0) return;
            for (int j = 0; j < mElementList.Count; j++) {
                if (mElementList[j].mPrimitive != null) {
                    mElementList[j].mPrimitive.mSurfaceVertex = mSurfaceVertex;
                    mElementList[j].mPrimitive.mWireFrame = mWireFrame;
                    mElementList[j].mPrimitive.createSurfaceData();
                    mElementList[j].mPrimitive.createVertexData();
                }
            }
        }

        /// <summary>
        /// ElementデータをSurfaceDataに変換
        /// </summary>
        /// <returns>SurfaceDataリスト</returns>
        public List<SurfaceData> getSurfaceData()
        {
            List<SurfaceData> surfaveDataList = new List<SurfaceData>();
            for (int i = 0;i < mElementList.Count;i++) {
                if (mElementList[i].isDraw(mLayer) && mElementList[i].mDisp3D)
                    surfaveDataList.AddRange(mElementList[i].mPrimitive.mSurfaceDataList);
            }
            return surfaveDataList;
        }

        /// <summary>
        /// エレメントの検索
        /// </summary>
        /// <param name="b">検索領域</param>
        /// <param name="face">表示面</param>
        /// <returns>検索リスト</returns>
        public List<int> findIndex(Box b, FACE3D face)
        {
            List<int> picks = new List<int>();
            if (face == FACE3D.NON)
                return picks;
            for (int i = 0; i < mElementList.Count; i++) {
                if (mElementList[i].pickChk(mLayer, b, face))
                    picks.Add(i);
            }
            return picks;
        }

        /// <summary>
        /// 表示要素数
        /// </summary>
        /// <returns>要素数</returns>
        public int getElementCount()
        {
            int count = 0;
            for (int i = 0; i < mElementList.Count; i++) {
                if (mElementList[i].isDraw(mLayer))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// システム設定
        /// </summary>
        public bool setSystemProperty()
        {
            SysPropertyDlg dlg = new SysPropertyDlg();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mArcDivideAngle = mArcDivideAng;
            dlg.mRevolutionDivideAngle = mRevolutionDivideAng;
            dlg.mSweepDivideAngle = mSweepDivideAng;
            dlg.mSurfaceVertex = mSurfaceVertex;
            dlg.mWireFrame = mWireFrame;
            dlg.mBackColor = mMainWindow.mDraw.mBaseBackColor;
            dlg.mDataFolder = mMainWindow.mFileData.mBaseDataFolder;
            dlg.mBackupFolder = mMainWindow.mFileData.mBackupFolder;
            dlg.mDiffTool = mMainWindow.mFileData.mDiffTool;
            dlg.mFileData = mMainWindow.mFileData;
            if (dlg.ShowDialog() == true) {
                mArcDivideAng = dlg.mArcDivideAngle;
                mRevolutionDivideAng = dlg.mRevolutionDivideAngle;
                mSweepDivideAng = dlg.mSweepDivideAngle;
                if(mSurfaceVertex != dlg.mSurfaceVertex
                     || mWireFrame != dlg.mWireFrame) {
                    mSurfaceVertex = dlg.mSurfaceVertex;
                    mWireFrame = dlg.mWireFrame;
                    reCreate();
                }
                mMainWindow.mDraw.mBaseBackColor = dlg.mBackColor;
                if (mMainWindow.mFileData.mBaseDataFolder != dlg.mDataFolder) {
                    mMainWindow.mFileData.setBaseDataFolder(dlg.mDataFolder, false);
                    mMainWindow.reloadDataFileList();
                }
                mMainWindow.mFileData.mBackupFolder = dlg.mBackupFolder;
                mMainWindow.mFileData.mDiffTool = dlg.mDiffTool;
                mOperationCount++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 図面属性を文字列配列リストに変換
        /// </summary>
        /// <returns>文字列配列リスト</returns>
        public List<string[]> toDataList()
        {
            List<string[]> list = new List<string[]>();
            string[] buf = { "DataManage" };
            list.Add(buf);
            buf = new string[] { "AppName", mMainWindow.mAppName };
            list.Add(buf);
            buf = new string[] { "PrimitiveBrush", ylib.getBrushName(mPrimitiveBrush) };
            list.Add(buf);
            buf = new string[] { "Face", mFace.ToString() };
            list.Add(buf);
            buf = new string[] { "ArcDivideAngle", mArcDivideAng.ToString() };
            list.Add(buf);
            buf = new string[] { "RevolutionDivideAngle", mRevolutionDivideAng.ToString() };
            list.Add(buf);
            buf = new string[] { "SweepDivideAngle", mSweepDivideAng.ToString() };
            list.Add(buf);
            buf = new string[] { "SurfaceVertex", mSurfaceVertex.ToString() };
            list.Add(buf);
            buf = new string[] { "WireFrame", mWireFrame.ToString() };
            list.Add(buf);
            buf = new string[] { "ZumenComment", mZumenComment };
            list.Add(buf);
            if (mArea != null) {
                buf = new string[] { "Area",
                    mArea.mMin.x.ToString(), mArea.mMin.y.ToString(), mArea.mMin.z.ToString(),
                    mArea.mMax.x.ToString(), mArea.mMax.y.ToString(), mArea.mMax.z.ToString(),
                };
                list.Add(buf);
            }
            list.AddRange(mLayer.toDataList());
            buf = new string[] { "DataManageEnd" };
            list.Add(buf);
            return list;
        }

        /// <summary>
        /// 図面属性を設定
        /// </summary>
        /// <param name="dataList">文字列配列リスト</param>
        /// <param name="sp">リスト開始位置</param>
        /// <returns>リスト終了位置</returns>
        public int setDataList(List<string[]> dataList, int sp)
        {
            string appName = "";
            while (sp < dataList.Count) {
                string[] buf = dataList[sp++];
                if (buf[0] == "AppName") {
                    appName = buf[1];
                } else if (buf[0] == "PrimitiveBrush") {
                    mPrimitiveBrush = ylib.getBrsh(buf[1]);
                } else if (buf[0] == "Face") {
                    mFace = (FACE3D)Enum.Parse(typeof(FACE3D), buf[1]);
                } else if (buf[0] == "ArcDivideAngle") {
                    mArcDivideAng = ylib.doubleParse(buf[1]);
                } else if (buf[0] == "RevolutionDivideAngle") {
                    mRevolutionDivideAng = ylib.doubleParse(buf[1]);
                } else if (buf[0] == "SweepDivideAngle") {
                    mSweepDivideAng = ylib.doubleParse(buf[1]);
                } else if (buf[0] == "SurfaceVertex") {
                    mSurfaceVertex = ylib.boolParse(buf[1]);
                } else if (buf[0] == "WireFrame") {
                    mWireFrame = ylib.boolParse(buf[1]);
                } else if (buf[0] == "ZumenComment") {
                    mZumenComment = buf[1];
                } else if (buf[0] == "Area" && buf.Length == 7) {
                    mArea = new Box3D();
                    mArea.mMin.x = ylib.doubleParse(buf[1]);
                    mArea.mMin.y = ylib.doubleParse(buf[2]);
                    mArea.mMin.z = ylib.doubleParse(buf[3]);
                    mArea.mMax.x = ylib.doubleParse(buf[4]);
                    mArea.mMax.y = ylib.doubleParse(buf[5]);
                    mArea.mMax.z = ylib.doubleParse(buf[6]);
                    if (mArea.isNaN() || mArea.isEmpty())
                        mArea = new Box3D(10);
                } else if (buf[0] == "Layer") {
                    sp = mLayer.setDataList(dataList, sp);
                } else if (buf[0] == "DataManageEnd") {
                    break;
                }
            }
            if (appName == mMainWindow.mAppName || appName == "")
                return sp;
            return -1;
        }

        /// <summary>
        /// ファイルから読み込む
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void loadData(string path)
        {
            if (!File.Exists(path))
                return;
            List<string[]> dataList = ylib.loadCsvData(path);
            if (0 == dataList.Count || dataList[0][0] != "DataManage")
                return;
            clear();
            Element element;
            int sp = 0;
            while (0 <= sp && sp < dataList.Count) {
                string[] buf = dataList[sp++];
                if (buf[0] == "Element") {
                    element = new Element(mLayerSize);
                    sp = element.setDataList(dataList, sp, mWireFrame, mSurfaceVertex);
                    if (element.mPrimitive != null && 0 < element.mPrimitive.mSurfaceDataList.Count) {
                        mElementList.Add(element);
                    }
                } else if (buf[0] == "DataManage") {
                    sp = setDataList(dataList, sp);
                } else if (buf[0] == "DataDraw") {
                    sp = mMainWindow.mDraw.setDataList(dataList, sp);
                }
            }
            updateData();
            mFirstEntityCount = mElementList.Count;
        }

        /// <summary>
        /// ファイルに保存
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void saveData(string path)
        {
            if (path.Length == 0 ||
                (mFirstEntityCount == mElementList.Count && 0 == mOperationCount))
                return;
            List<string[]> dataList = new List<string[]>();
            dataList.AddRange(toDataList());
            dataList.AddRange(mMainWindow.mDraw.toDataList());
            for (int i = 0; i < mElementList.Count; i++) {
                if (!mElementList[i].mRemove && mElementList[i].mPrimitive != null
                    && mElementList[i].mLinkNo < 0)
                    dataList.AddRange(mElementList[i].toDataList());
            }
            ylib.saveCsvData(path, dataList);
        }
    }
}
