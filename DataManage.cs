using CoreLib;
using System.IO;
using System.Windows;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Mini3DCad
{
    /// <summary>
    /// ピックデータ
    /// </summary>
    public class PickData
    {
        public int mElementNo;                  //  要素No
        public PointD mPos;                     //  ピック位置
        public FACE3D mDispMode;                //  表示面

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="no">要素No</param>
        /// <param name="pos">ピック位置</param>
        /// <param name="dispMode">表示面</param>
        public PickData(int no, PointD pos, FACE3D dispMode)
        {
            mElementNo = no;
            mPos = pos;
            mDispMode = dispMode;
        }
    }

    public class DataManage
    {
        public FACE3D mFace = FACE3D.XY;                            //  Primitive 作成面
        public Brush mPrimitiveBrush = Brushes.Green;               //  Primitiveの色設定
        public List<PointD> mLocList = new();                       //  ロケイトの保存
        public List<PickData> mPickElement = new List<PickData>();  //  ピックエレメント
        public List<Element> mElementList = new List<Element>();    //  エレメントリスト
        public Box3D mArea;                                         //  要素領域
        public int mOperationCount = 0;                             //  操作回数

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
        }

        /// <summary>
        /// Elementデータリストをクリア
        /// </summary>
        public void clear()
        {
            mElementList.Clear();
        }

        /// <summary>
        /// データの登録
        /// </summary>
        /// <param name="ope">処理の種別</param>
        /// <param name="last">ロケイトの完了</param>
        /// <returns></returns>
        public bool defineData(OPERATION ope, bool last = false)
        {
            mOperationCount++;
            switch (ope) {
                case OPERATION.line:
                    if (mLocList.Count == 2) {
                        addLine(mLocList[0], mLocList[1]);
                        mLocList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.circle:
                    if (mLocList.Count == 2) {
                        addCircle(mLocList[0], mLocList[1]);
                        mLocList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.arc:
                    if (mLocList.Count == 3) {
                        addArc(mLocList[0], mLocList[2], mLocList[1]);
                        mLocList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.polyline:
                    if (1 < mLocList.Count && last) {
                        addPolyline(mLocList);
                        mLocList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.rect:
                    if (mLocList.Count == 2) {
                        addRect(mLocList[0], mLocList[1]);
                        mLocList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.polygon:
                    if (2 < mLocList.Count && last) {
                        addPolygon(mLocList);
                        mLocList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.translate:
                    if (mLocList.Count == 2 && 0 < mPickElement.Count) {
                        translate(mPickElement, mLocList[0], mLocList[1]);
                        mLocList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.copyTranslate:
                    if (mLocList.Count == 2 && 0 < mPickElement.Count) {
                        translate(mPickElement, mLocList[0], mLocList[1], true);
                        mLocList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.rotate:
                    if (mLocList.Count == 3 && 0 < mPickElement.Count) {
                        rotate(mPickElement, mLocList);
                        mLocList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.copyRotate:
                    if (mLocList.Count == 3 && 0 < mPickElement.Count) {
                        rotate(mPickElement, mLocList, true);
                        mLocList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.divide:
                    if (mLocList.Count == 1 && 0 < mPickElement.Count) {
                        divide(mPickElement, mLocList);
                        mLocList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.extrusion:
                    if (mLocList.Count == 2 && 0 < mPickElement.Count) {
                        extrusion(mPickElement, mLocList[0], mLocList[1]);
                        mLocList.Clear();
                    } else
                        return false;
                    break;
                case OPERATION.revolution:
                    if (1 < mPickElement.Count) {
                        revolution(mPickElement);
                        mLocList.Clear();
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
        /// 線分追加
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        public void addLine(PointD sp, PointD ep)
        {
            Element element = new Element();
            element.mName = "線分";
            element.mPrimitive = createLine(sp, ep);
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
        }

        /// <summary>
        /// 線分の追加
        /// </summary>
        /// <param name="line">3D線分</param>
        /// <param name="color">線分カラー</param>
        /// <param name="faceColor">フェイスカラー</param>
        public void addLine(Line3D line, Brush color = null, Brush faceColor = null)
        {
            Element element = new Element();
            element.mName = "線分";
            element.mPrimitive = createLine(line);
            if (color != null)
                element.mPrimitive.mLineColor = color;
            if (faceColor != null)
                element.mPrimitive.mFaceColors[0] = faceColor;
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
        }

        /// <summary>
        /// 円の追加
        /// </summary>
        /// <param name="cp">中心点</param>
        /// <param name="ep">円弧上の点</param>
        public void addCircle(PointD cp, PointD ep)
        {
            Element element = new Element();
            element.mName = "円";
            element.mPrimitive = createCircle(cp, ep);
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
        }
        /// <summary>
        /// 円弧の追加
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="mp">中間点</param>
        /// <param name="ep">終点</param>
        public void addArc(PointD sp, PointD mp, PointD ep)
        {
            try {
                Element element = new Element();
                element.mName = "円弧";
                element.mPrimitive = createArc(sp, mp, ep);
                element.mOperationNo = mOperationCount;
                element.update3DData();
                mElementList.Add(element);
            } catch (Exception e) {
                System.Diagnostics.Debug.WriteLine($"addArc: {e.Message}");
            }
        }

        /// <summary>
        /// ポリラインの追加
        /// </summary>
        /// <param name="pist">2D座標点リスト</param>
        public void addPolyline(List<PointD> pist)
        {
            Element element = new Element();
            element.mName = "ポリライン";
            element.mPrimitive = createPolyline(pist);
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
        }

        /// <summary>
        /// ポリラインの追加
        /// </summary>
        /// <param name="pist">3D座標点リスト</param>
        /// <param name="color">線分カラー</param>
        /// <param name="faceColor">フェイスカラー</param>
        public void addPolyline(List<Point3D> pist, Brush color = null, Brush faceColor = null)
        {
            Element element = new Element();
            element.mName = "ポリライン";
            element.mPrimitive = createPolyline(pist);
            if (color != null)
                element.mPrimitive.mLineColor = color;
            if (faceColor != null)
                element.mPrimitive.mFaceColors[0] = faceColor;
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
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
            Element element = new Element();
            element.mName = "四角形";
            element.mPrimitive = createPolygon(plist);
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
        }

        /// <summary>
        /// ポリゴンの追加
        /// </summary>
        /// <param name="pist">座標点リスト</param>
        public void addPolygon(List<PointD> pist)
        {
            Element element = new Element();
            element.mName = "ポリゴン";
            element.mPrimitive = createPolygon(pist);
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
        }

        /// <summary>
        /// ポリゴンの追加
        /// </summary>
        /// <param name="polygon">3D座標点リスト</param>
        /// <param name="color">線分カラー</param>
        /// <param name="faceColor">フェイスカラー</param>
        public void addPolygon(Polygon3D polygon, Brush color = null, Brush faceColor = null)
        {
            Element element = new Element();
            element.mName = "ポリゴン";
            element.mPrimitive = createPolygon(polygon);
            if (color != null)
                element.mPrimitive.mLineColor = color;
            if (faceColor != null)
                element.mPrimitive.mFaceColors[0] = faceColor;
            element.mOperationNo = mOperationCount;
            element.update3DData();
            mElementList.Add(element);
        }

        /// <summary>
        /// Link要素を追加
        /// </summary>
        /// <param name="linkNo">リンク先No</param>
        public void addLink(int linkNo)
        {
            Element element = new Element();
            element.mName = "リンク";
            element.mLinkNo = linkNo;
            element.mOperationNo = mOperationCount;
            mElementList.Add(element);
        }

        /// <summary>
        /// 移動処理
        /// </summary>
        /// <param name="picks">ピック要素</param>
        /// <param name="sp">移動始点</param>
        /// <param name="ep">移動終点</param>
        /// <param name="copy">コピーの有無</param>
        public void translate(List<PickData> picks, PointD sp, PointD ep, bool copy = false)
        {
            Point3D v = new Point3D(ep, mFace) - new Point3D(sp, mFace);
            foreach (var pick in picks) {
                Element element = mElementList[pick.mElementNo].toCopy();
                element.mPrimitive.translate(v);
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
        /// 回転処理
        /// </summary>
        /// <param name="picks"><ピック要素/param>
        /// <param name="locList">座標リスト</param>
        /// <param name="copy">コピーの有無</param>
        public void rotate(List<PickData> picks, List<PointD> locList, bool copy = false)
        {
            double ang = locList[0].angle2(locList[1], locList[2]);
            Point3D cp = new Point3D(locList[0], mFace);
            foreach (var pick in picks) {
                Element element = mElementList[pick.mElementNo].toCopy();
                element.mPrimitive.rotate(cp, -ang, mFace);
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
        /// 結合処理(2要素を接続する))
        /// </summary>
        /// <param name="picks">ピックリスト</param>
        public void connect(List<PickData> picks)
        {
            if (picks.Count < 2)
                return;

            Element ele0 = mElementList[picks[0].mElementNo];
            if (picks[0].mElementNo == picks[1].mElementNo) {
                //  同一要素をピック ポリゴン化
                if (ele0.mPrimitive.mPrimitiveId == PrimitiveId.Polyline ||
                    ele0.mPrimitive.mPrimitiveId == PrimitiveId.Arc) {
                    Polyline3D polyline = ele0.mPrimitive.getVertexList();
                    Element element = new Element();
                    element.mName = "ポリゴン化";
                    element.mPrimitive = new PolygonPrimitive(polyline, ele0.mPrimitive.mLineColor, mFace);
                    element.mPrimitive.copyProperty(ele0.mPrimitive);
                    element.mPrimitive.mPrimitiveId = PrimitiveId.Polygon;
                    element.mOperationNo = mOperationCount;
                    element.update3DData();
                    mElementList.Add(element);
                    ele0.mRemove = true;
                    addLink(picks[0].mElementNo);
                }
            } else {
                Element ele1 = mElementList[picks[1].mElementNo];
                if ((ele0.mPrimitive.mPrimitiveId == PrimitiveId.Line ||
                    ele0.mPrimitive.mPrimitiveId == PrimitiveId.Arc ||
                    ele0.mPrimitive.mPrimitiveId == PrimitiveId.Polyline) &&
                    (ele1.mPrimitive.mPrimitiveId == PrimitiveId.Line ||
                    ele1.mPrimitive.mPrimitiveId == PrimitiveId.Arc ||
                    ele1.mPrimitive.mPrimitiveId == PrimitiveId.Polyline)) {

                    Polyline3D polyline = new Polyline3D();
                    Polyline3D pl0 = ele0.mPrimitive.getVertexList();
                    Polyline3D pl1 = ele1.mPrimitive.getVertexList();
                    polyline.add(pl0.toPoint3D(), picks[0].mPos, mFace, false);
                    polyline.add(pl1.toPoint3D(), picks[1].mPos, mFace, true);
                    polyline.squeeze();
                    Element element = new Element();
                    element.mName = "結合ポリライン";
                    element.mPrimitive = new PolylinePrimitive(polyline, ele1.mPrimitive.mLineColor);
                    element.mPrimitive.copyProperty(ele1.mPrimitive);
                    element.mPrimitive.mPrimitiveId = PrimitiveId.Polyline;
                    element.mOperationNo = mOperationCount;
                    element.update3DData();
                    mElementList.Add(element);

                    ele0.mRemove = true;
                    addLink(picks[0].mElementNo);
                    ele1.mRemove = true;
                    addLink(picks[1].mElementNo);
                }
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

                Element element = new Element();
                element.mName = "押出";
                element.mPrimitive = push;
                element.mOperationNo = mOperationCount;
                element.update3DData();
                mElementList.Add(element);

                mElementList[pick.mElementNo].mRemove = true;
                addLink(pick.mElementNo);
            }
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
            Brush color = centerLinePrimitive.mLineColor;

            Line3D centerLine;
            List<Point3D> outline;
            if (centerLinePrimitive.mPrimitiveId == PrimitiveId.Line) {
                LinePrimitive linePrimitive = (LinePrimitive)centerLinePrimitive;
                centerLine = linePrimitive.mLine.toCopy();
            } else if (centerLinePrimitive.mPrimitiveId == PrimitiveId.Polyline) {
                PolylinePrimitive polylinePrimitive = (PolylinePrimitive)centerLinePrimitive;
                centerLine = new Line3D(polylinePrimitive.mPolyline.toPoint3D(0), polylinePrimitive.mPolyline.toPoint3D(1));
            } else
                return;
            if (outlinePrimitive.mPrimitiveId == PrimitiveId.Line) {
                LinePrimitive linePrimitive = (LinePrimitive)outlinePrimitive;
                outline = linePrimitive.mLine.toPoint3D();
            } else if (outlinePrimitive.mPrimitiveId == PrimitiveId.Polyline) {
                PolylinePrimitive polylinePrimitive = (PolylinePrimitive)outlinePrimitive;
                outline = polylinePrimitive.mPolyline.toPoint3D();
            } else
                return;

            RevolutionPrimitive revolution = new RevolutionPrimitive(centerLine, outline, color, true, mFace);
            if (revolution == null)
                return;
            revolution.copyProperty(outlinePrimitive);
            revolution.mPrimitiveId = PrimitiveId.Revolution;
            revolution.mPick = false;

            Element element = new Element();
            element.mName = "回転体";
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
        /// 押出や回転体を解除して元のポリゴンや線分に戻す
        /// </summary>
        /// <param name="picks"></param>
        public void release(List<PickData> picks)
        {
            for (int i = 0; i < picks.Count; i++) {
                Element element = mElementList[picks[i].mElementNo].toCopy();
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Extrusion) {
                    //  押出解除
                    ExtrusionPrimitive extrusion = (ExtrusionPrimitive) element.mPrimitive;
                    addPolygon(extrusion.mPolygon,extrusion.mLineColor, extrusion.mFaceColors[0]);
                } else if (element.mPrimitive.mPrimitiveId == PrimitiveId.Revolution) {
                    //  回転体解除
                    RevolutionPrimitive revolution = (RevolutionPrimitive) element.mPrimitive;
                    Line3D line = revolution.mCenterLine.toCopy();
                    addLine(line);
                    Polyline3D polyline = new Polyline3D(revolution.mOutLine);
                    addPolyline(polyline.toPoint3D(), revolution.mLineColor, revolution.mFaceColors[0]);
                } else {
                    continue;
                }
                mElementList[picks[i].mElementNo].mRemove = true;
                addLink(picks[i].mElementNo);
            }
        }

        /// <summary>
        /// Elementの色変更
        /// </summary>
        /// <param name="picks"></param>
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
                dlg.mFaceColor = element.mPrimitive.mFaceColors[0];
                dlg.mFaceColorNull = element.mPrimitive.mFaceColors[0] == null;
                dlg.mDisp3D = element.mDisp3D;
                dlg.mBothShading = element.mBothShading;
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Arc) {
                    ArcPrimitive arc = (ArcPrimitive)element.mPrimitive;
                    dlg.mDivideAngOn = true;
                    dlg.mDivideAng = ylib.R2D(arc.mDivideAngle);
                }
                if (element.mPrimitive.mPrimitiveId == PrimitiveId.Polyline ||
                    element.mPrimitive.mPrimitiveId == PrimitiveId.Polygon ||
                    element.mPrimitive.mPrimitiveId == PrimitiveId.Extrusion) {
                    dlg.mReverseOn = true;
                    dlg.mReverse = false;
                }
                if (dlg.ShowDialog() == true) {
                    element.mName = dlg.mName;
                    element.mPrimitive.mLineColor = dlg.mLineColor;
                    if (dlg.mFaceColorNull)
                        element.mPrimitive.mFaceColors[0] = null;
                    else
                        element.mPrimitive.mFaceColors[0] = dlg.mFaceColor;
                    element.mDisp3D = dlg.mDisp3D;
                    element.mBothShading = dlg.mBothShading;
                    element.mOperationNo = mOperationCount;
                    element.update3DData();
                    if (element.mPrimitive.mPrimitiveId == PrimitiveId.Arc) {
                        ArcPrimitive arc = (ArcPrimitive)element.mPrimitive;
                        arc.mDivideAngle = ylib.D2R(dlg.mDivideAng);
                    }
                    if (dlg.mReverse) {
                        if (element.mPrimitive.mPrimitiveId == PrimitiveId.Polygon) {
                            PolygonPrimitive polygon = (PolygonPrimitive)element.mPrimitive;
                            polygon.mPolygon.mPolygon.Reverse();
                        }
                        if (element.mPrimitive.mPrimitiveId == PrimitiveId.Extrusion) {
                            ExtrusionPrimitive push = (ExtrusionPrimitive)element.mPrimitive;
                            push.mPolygon.mPolygon.Reverse();
                        }
                    }
                    element.mPrimitive.createVertexList();
                    mElementList.Add(element);
                    mElementList[picks[i].mElementNo].mRemove = true;
                    addLink(picks[i].mElementNo);
                }
            }
        }

        /// <summary>
        /// 線分プリミティブの作成
        /// </summary>
        /// <param name="sp">始点</param>
        /// <param name="ep">終点</param>
        /// <returns>プリミティブ</returns>
        public Primitive createLine(PointD sp, PointD ep)
        {
            LinePrimitive line = new LinePrimitive(sp, ep, mFace);
            line.mLineColor = mPrimitiveBrush;
            line.mFaceColors[0] = mPrimitiveBrush;
            return line;
        }

        /// <summary>
        /// 線分プリミティブの作成
        /// </summary>
        /// <param name="l"></param>
        /// <returns>プリミティブ</returns>
        public Primitive createLine(Line3D l)
        {
            LinePrimitive line = new LinePrimitive(l, mFace);
            line.mLineColor = mPrimitiveBrush;
            line.mFaceColors[0] = mPrimitiveBrush;
            return line;
        }

        /// <summary>
        /// 円プリミティブの作成
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="ep"></param>
        /// <returns></returns>
        public Primitive createCircle(PointD sp, PointD ep)
        {
            ArcPrimitive arc = new ArcPrimitive(sp, sp.length(ep), mFace);
            arc.mLineColor = mPrimitiveBrush;
            arc.mFaceColors[0] = mPrimitiveBrush;
            return arc;
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
            ArcD arc2D = new ArcD(sp, mp, ep);
            ArcPrimitive arc = new ArcPrimitive(arc2D, mFace);
            arc.mLineColor = mPrimitiveBrush;
            arc.mFaceColors[0] = mPrimitiveBrush;
            return arc;
        }

        /// <summary>
        /// ポリラインプリミティブの作成
        /// </summary>
        /// <param name="plist">2D座標点リスト</param>
        /// <returns>プリミティブ</returns>
        public Primitive createPolyline(List<PointD> plist)
        {
            PolylinePrimitive polyline = new PolylinePrimitive(plist, mPrimitiveBrush, mFace);
            polyline.mLineColor = mPrimitiveBrush;
            polyline.mFaceColors[0] = mPrimitiveBrush;
            return polyline;
        }

        /// <summary>
        /// ポリラインプリミティブの作成
        /// </summary>
        /// <param name="plist">3D座標点リスト</param>
        /// <returns></returns>
        public Primitive createPolyline(List<Point3D> plist)
        {
            PolylinePrimitive polyline = new PolylinePrimitive(plist, mPrimitiveBrush, mFace);
            polyline.mLineColor = mPrimitiveBrush;
            polyline.mFaceColors[0] = mPrimitiveBrush;
            return polyline;
        }

        /// <summary>
        /// ポリゴンプリミティブの作成
        /// </summary>
        /// <param name="plist">2D座標点リスト</param>
        /// <returns>プリミティブ</returns>
        public Primitive createPolygon(List<PointD> plist)
        {
            PolygonPrimitive polygon = new PolygonPrimitive(plist, mPrimitiveBrush, mFace);
            polygon.mLineColor = mPrimitiveBrush;
            polygon.mFaceColors[0] = mPrimitiveBrush;
            return polygon;
        }

        /// <summary>
        /// ポリゴンプリミティブの作成
        /// </summary>
        /// <param name="plist">3D座標リスト</param>
        /// <returns>ポリゴンプリミティブ</returns>
        public Primitive createPolygon(Polygon3D plist)
        {
            PolygonPrimitive polygon = new PolygonPrimitive(plist, mPrimitiveBrush, mFace);
            return polygon;
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
                extrusion = new ExtrusionPrimitive(primitive.mSurfaceDataList[0].mVertexList, primitive.mPrimitiveFace, v, primitive.mLineColor, false, mFace);
            } else if (primitive.mPrimitiveId == PrimitiveId.Polygon) {
                PolygonPrimitive polygon = (PolygonPrimitive)primitive;
                extrusion = new ExtrusionPrimitive(polygon.mPolygon, polygon.mPrimitiveFace, v, primitive.mLineColor, true, mFace);
            } else {
                extrusion = new ExtrusionPrimitive(primitive.mSurfaceDataList[0].mVertexList, primitive.mPrimitiveFace, v, primitive.mLineColor, false, mFace);
            }
            extrusion.copyProperty(primitive);
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
            Polyline3D polyline = polygonPrimitive.mPolygon.divide(locPos, mFace);
            if (polyline == null)
                return false;
            Element ele = new Element();
            ele.mName = "分割ポリライン";
            ele.mPrimitive = new PolylinePrimitive(polyline, polygonPrimitive.mLineColor);
            ele.mPrimitive.copyProperty(polygonPrimitive);
            ele.mPrimitive.mPrimitiveId = PrimitiveId.Polyline;
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
                Element ele1 = new Element();
                ele1.mName = "分割ポリライン";
                ele1.mPrimitive = new PolylinePrimitive(polylines[1].mPolyline, polylinePrimitive.mLineColor);
                ele1.mPrimitive.copyProperty(polylinePrimitive);
                ele1.mOperationNo = mOperationCount;
                ele1.update3DData();
                mElementList.Add(ele1);
            }
            if (0 < polylines.Count) {
                Element ele1 = new Element();
                ele1.mName = "分割ポリライン";
                ele1.mPrimitive = new PolylinePrimitive(polylines[0].mPolyline, polylinePrimitive.mLineColor);
                ele1.mPrimitive.copyProperty(polylinePrimitive);
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
                Element ele1 = new Element();
                ele1.mName = "分割線分";
                ele1.mPrimitive = new LinePrimitive(lines[1]);
                ele1.mPrimitive.copyProperty(linePrimitive);
                ele1.mOperationNo = mOperationCount;
                ele1.update3DData();
                mElementList.Add(ele1);
            }
            if (0 < lines.Count) {
                Element ele1 = new Element();
                ele1.mName = "分割線分";
                ele1.mPrimitive = new LinePrimitive(lines[0]);
                ele1.mPrimitive.copyProperty(linePrimitive);
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
            List<Arc3D> arcs = arcPrimitive.mArc.divide(locPos, mFace);
            if (1 < arcs.Count) {
                Element ele1 = new Element();
                ele1.mName = "分割円弧";
                ele1.mPrimitive = new ArcPrimitive(arcs[1]);
                ele1.mPrimitive.copyProperty(arcPrimitive);
                ele1.mOperationNo = mOperationCount;
                ele1.update3DData();
                mElementList.Add(ele1);
            }
            if (0 < arcs.Count) {
                Element ele1 = new Element();
                ele1.mName = "分割円弧";
                ele1.mPrimitive = new ArcPrimitive(arcs[0]);
                ele1.mPrimitive.copyProperty(arcPrimitive);
                ele1.mOperationNo = mOperationCount;
                ele1.update3DData();
                mElementList.Add(ele1);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 要素情報表示
        /// </summary>
        /// <param name="picks"></param>
        public void info(List<PickData> picks)
        {
            for (int i = 0; i < picks.Count; i++) {
                string buf = mElementList[picks[i].mElementNo].propertyInfo();
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
            updateData();
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
            for (int i = 0; i < mElementList.Count; i++) {
                if (!mElementList[i].mRemove && 0 > mElementList[i].mLinkNo) {
                    mArea = mElementList[i].mArea;
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
        /// ElementデータをSurfaceDataに変換
        /// </summary>
        /// <returns>SurfaceDataリスト</returns>
        public List<SurfaceData> getSurfaceData()
        {
            List<SurfaceData> surfaveDataList = new List<SurfaceData>();
            for (int i = 0;i < mElementList.Count;i++) {
                if (!mElementList[i].mRemove && 0 > mElementList[i].mLinkNo &&
                    mElementList[i].mDisp3D)
                surfaveDataList.AddRange(mElementList[i].mPrimitive.mSurfaceDataList);
            }
            return surfaveDataList;
        }

        public void autoLoc(PointD pickPos, List<int> picks)
        {
            PointD? wp = null;
            if (ylib.onControlKey()) {
                //  Ctrlキーでのメニュー表示で位置を選定
                //wp = locSelect(pickPos, picks);
            } else {
                //  ピックされているときは位置を自動判断
                if (picks.Count == 1) {
                    //  ピックされているときは位置を自動判断
                    wp = autoLoc(pickPos, picks[0]);
                } else if (2 <= picks.Count) {
                    //  2要素の時は交点位置
                    wp = intersectionLoc(picks[0], picks[1], pickPos);
                    if (wp == null)
                        wp = autoLoc(pickPos, picks[0]);
                    if (wp == null)
                        wp = autoLoc(pickPos, picks[1]);
                }
            }
            if (wp != null)
                mLocList.Add(wp);
        }

        private PointD autoLoc(PointD pos, int entNo = -1)
        {
            return mElementList[entNo].mPrimitive.nearPoint(pos, 4, mFace);
        }

        private PointD intersectionLoc(int entNo0, int entNo1, PointD pos)
        {
            return null;
        }

        /// <summary>
        /// ピック要素番号の取得
        /// </summary>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="pickSize">ピックボックスサイズ</param>
        /// <returns></returns>
        public List<int> getPickNo(PointD pickPos, double pickSize)
        {
            Box b = new Box(pickPos, pickSize);
            return findIndex(b, mFace);
        }

        /// <summary>
        /// 領域指定のピック処理
        /// </summary>
        /// <param name="pickArea">ピック領域</param>
        /// <returns>ピックリスト</returns>
        public List<int> getPickNo(Box pickArea)
        {
            return findIndex(pickArea, mFace);
        }

        /// <summary>
        /// 要素ピック(ピックした要素を登録)
        /// </summary>
        /// <param name="wpos">ピック位置</param>
        /// <param name="face">表示面</param>
        public void pickElement(PointD wpos, List<int> picks, OPEMODE opeMode)
        {
            if (0 < picks.Count) {
                //  要素選択
                if (opeMode == OPEMODE.areaPick) {
                    for (int i = 0; i < picks.Count; i++)
                        addPickList(picks[i], wpos, mFace);
                } else {
                    int pickNo = pickSelect(picks);
                    if (0 <= pickNo) {
                        //  ピック要素の登録
                        addPickList(pickNo, wpos, mFace);
                    }
                }
            }
        }

        /// <summary>
        /// 複数ピック時の選択
        /// </summary>
        /// <param name="picks">ピックリスト</param>
        /// <returns>要素番号</returns>
        private int pickSelect(List<int> picks)
        {
            if (picks.Count == 1)
                return picks[0];
            List<int> sqeezePicks = picks.Distinct().ToList();
            List<string> menu = new List<string>();
            for (int i = 0; i < sqeezePicks.Count; i++) {
                Element ele = mElementList[sqeezePicks[i]];
                menu.Add($"{sqeezePicks[i]} {ele.getSummary()}");
            }
            MenuDialog dlg = new MenuDialog();
            dlg.mMainWindow = mMainWindow;
            dlg.mHorizontalAliment = 1;
            dlg.mVerticalAliment = 1;
            dlg.mOneClick = true;
            dlg.mMenuList = menu;
            dlg.ShowDialog();
            if (dlg.mResultMenu == "")
                return -1;
            else
                return ylib.string2int(dlg.mResultMenu);
        }

        /// <summary>
        /// ピック要素をピックリストに追加
        /// </summary>
        /// <param name="pickNo">要素番号</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">表示面</param>
        public void addPickList(int pickNo, PointD pickPos, FACE3D face)
        {
            mPickElement.Add(new PickData(pickNo, pickPos, face));
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
            for (int i = 0; i < mElementList.Count; i++) {
                if (!mElementList[i].mRemove && mElementList[i].mLinkNo < 0) {
                    if (!b.outsideChk(mElementList[i].mArea.toBox(face))) {
                        if (mElementList[i].mPrimitive.pickChk(b, face))
                            picks.Add(i);
                    }
                }
            }
            return picks;
        }

        /// <summary>
        /// ピックフラグの設定
        /// </summary>
        public void setPick()
        {
            for (int i = 0; i < mPickElement.Count; i++)
                mElementList[mPickElement[i].mElementNo].mPrimitive.mPick = true;
        }

        /// <summary>
        /// ピックフラグの全解除
        /// </summary>
        public void pickReset()
        {
            for (int i = 0; i < mPickElement.Count; i++)
                mElementList[mPickElement[i].mElementNo].mPrimitive.mPick = false;
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
            buf = new string[] { "PrimitiveBrush", ylib.getBrushName(mPrimitiveBrush) };
            list.Add(buf);
            buf = new string[] { "Face", mFace.ToString() };
            list.Add(buf);
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
            while (sp < dataList.Count) {
                string[] buf = dataList[sp++];
                if (buf[0] == "PrimitiveBrush") {
                    mPrimitiveBrush = ylib.getBrsh(buf[1]);
                } else if (buf[0] == "Face") {
                    mFace = (FACE3D)Enum.Parse(typeof(FACE3D), buf[1]);
                } else if (buf[0] == "DataManageEnd") {
                    break;
                }
            }
            return sp;
        }

        /// <summary>
        /// ファイルから読み込む
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void loadData(string path)
        {
            System.Diagnostics.Debug.WriteLine($"{path}");
            if (!File.Exists(path))
                return;
            List<string[]> dataList = ylib.loadCsvData(path);
            mElementList.Clear();
            Element element;
            int sp = 0;
            while (sp < dataList.Count) {
                string[] buf = dataList[sp++];
                if (buf[0] == "Element") {
                    element = new Element();
                    sp = element.setDataList(dataList, sp);
                    if (element.mPrimitive != null && 0 < element.mPrimitive.mSurfaceDataList.Count)
                        mElementList.Add(element);
                } else if (buf[0] == "DataManage") {
                    sp = setDataList(dataList, sp);
                } else if (buf[0] == "DataDraw") {
                    sp = mMainWindow.mDraw.setDataList(dataList, sp);
                }
            }
            updateData();
        }

        /// <summary>
        /// ファイルに保存
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void saveData(string path)
        {
            if (path.Length == 0)
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
