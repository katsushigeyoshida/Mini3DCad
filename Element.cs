using CoreLib;
using System.Globalization;

namespace Mini3DCad
{
    public class Element
    {
        public Dictionary<PrimitiveId, string> mPrimitiveName = new Dictionary<PrimitiveId, string>() {
            { PrimitiveId.Non, "" },
            { PrimitiveId.Point, "点" },
            { PrimitiveId.Line, "線分" },
            { PrimitiveId.Arc, "円弧" },
            { PrimitiveId.Polyline, "折線" },
            { PrimitiveId.Polygon, "多角形" },
            { PrimitiveId.Extrusion, "押出" },
            { PrimitiveId.Blend, "ブレンド" },
            { PrimitiveId.Revolution, "回転体" },
            { PrimitiveId.Sweep, "掃引" },
        };

        public string mName;                    //  エレメント名(任意)
        public Primitive mPrimitive;            //  プリミティブリスト
        public List<Surface> mSurfaceList;      //  サーフェスデータリスト(LINE/TRYANGLE)
        public bool mBothShading = true;        //  両面陰面判定の有無
        public bool mDisp3D = true;             //  3Dでの表示/非表示
        public bool mRemove = false;            //  削除フラグ
        public Box3D mArea;                     //  要素領域
        public int mOperationNo = -1;           //  操作位置
        public int mLinkNo = -1;                //  リンク先要素番号
        public byte[] mLayerBit;                //  レイヤーBit

        private YLib ylib = new YLib();

        public Element(int layersize)
        {
            mSurfaceList = new List<Surface>();
            mName = "要素名";
            mLayerBit = new byte[layersize / 8];
        }

        /// <summary>
        /// 全データクリア
        /// </summary>
        public void clear()
        {
            mSurfaceList = new List<Surface>();
        }

        /// <summary>
        /// 2D表示処理
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="face">表示面</param>
        public void draw2D(YWorldDraw draw, FACE3D face)
        {
            if (mArea != null && !draw.mClipBox.outsideChk(mArea.toBox(face)))
                mPrimitive.draw2D(draw, face);
        }

        /// <summary>
        /// コピーの作成
        /// </summary>
        /// <returns></returns>
        public Element toCopy()
        {
            Element element = new Element(mLayerBit.Length * 8);
            element.mName = mName;
            element.mPrimitive = mPrimitive.toCopy();
            element.mSurfaceList = mSurfaceList.ConvertAll(p => p.toCopy());
            element.mBothShading = mBothShading;
            element.mDisp3D = mDisp3D;
            element.mRemove = mRemove;
            element.mArea = mArea.toCopy();
            element.mOperationNo = mOperationNo;
            element.mLinkNo = mLinkNo;
            Array.Copy(mLayerBit, element.mLayerBit, mLayerBit.Length);
            return element;
        }

        /// <summary>
        /// Element属性をコピーする
        /// </summary>
        /// <param name="element"></param>
        /// <param name="dataList">データリスト</param>
        /// <param name="id">Primitive ID</param>
        public void copyProperty(Element element, bool dataList = false, bool id = false)
        {
            mName = element.mName;
            mPrimitive.copyProperty(element.mPrimitive, dataList, id);
            mBothShading = element.mBothShading;
            mDisp3D = element.mDisp3D;
        }

        /// <summary>
        /// レイヤーBitをコピー
        /// </summary>
        /// <param name="element">エレメント</param>
        public void copyLayer(Element element)
        {
            Array.Copy(element.mLayerBit, mLayerBit, mLayerBit.Length);
        }


        /// <summary>
        /// 表示条件の判定
        /// </summary>
        /// <returns>表示</returns>
        public bool isDraw(Layer layer)
        {
            if (!mRemove && mPrimitive != null &&
                mPrimitive.mPrimitiveId != PrimitiveId.Non &&
                mPrimitive.mPrimitiveId != PrimitiveId.Link &&
                (layer.bitAnd(mLayerBit) || layer.mLayerAll || layer.IsEmpty(mLayerBit)))
                return true;
            else
                return false;
        }

        /// <summary>
        /// ピックの絞り込み
        /// </summary>
        /// <param name="layer">レイヤ</param>
        /// <param name="b">ピック領域</param>
        /// <param name="face">2D平面</param>
        /// <returns>ピックの有無</returns>
        public bool pickChk(Layer layer, Box b, FACE3D face)
        {
            if (isDraw(layer) && face != FACE3D.NON) {
                if (!b.outsideChk(mArea.toBox(face))) {
                    if (mPrimitive.pickChk(b, face))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// エレメント情報
        /// </summary>
        /// <returns>文字列</returns>
        public string propertyInfo()
        {
            string buf = $"名称:[{mName}] 種類:[{mPrimitiveName[mPrimitive.mPrimitiveId]}]";
            buf += $" 2D色:[{ylib.getBrushName(mPrimitive.mLineColor)}] 3D色:[{ylib.getBrushName(mPrimitive.mFaceColors[0])}]";
            buf += $" 両面表示:[{mBothShading}] 3D表示:[{mDisp3D}] 反転:[{mPrimitive.mReverse}]";
            return buf;
        }

        /// <summary>
        /// エレメントの簡易情報
        /// </summary>
        /// <returns>文字列</returns>
        public string getSummary(string form = "F2")
        {
            return $"{mName} {mPrimitiveName[mPrimitive.mPrimitiveId]} {mPrimitive.dataSummary(form)}";
        }

        /// <summary>
        /// Primitiveデータ情報の取得
        /// </summary>
        /// <returns>文字列</returns>
        public string dataInfo()
        {
            string buf = ylib.insertLinefeed(mPrimitive.dataInfo("F2"), ",", 100);
            int count = mPrimitive.mSurfaceDataList.Select(x => x.mVertexList.Count).Sum();
            int vertexCount = mPrimitive.mVertexList.Select(x => x.Count).Sum();
            buf += $"\nSurfaceList {count} VertexList {vertexCount}";
            //List<string> list = mPrimitive.vertexInfo();
            //string s = string.Join(",", list);
            //buf += $"\n{ylib.insertLinefeed(s, ",", 100)}";
            return buf;
        }

        /// <summary>
        /// 3D VertexListをUPDATEする
        /// </summary>
        public void update3DData()
        {
            if (mPrimitive != null)
                mArea = mPrimitive.getArea();
        }

        /// <summary>
        /// Elementデータを文字列配列リストに変換
        /// </summary>
        /// <returns>文字列配列リスト</returns>
        public List<string[]> toDataList()
        {
            List<string[]> list = new List<string[]>();
            string[] buf = { "Element" };
            list.Add(buf);
            buf = new string[] { "Name", mName };
            list.Add(buf);
            list.Add(mPrimitive.toPropertyList());
            list.Add(mPrimitive.toDataList());
            if (mPrimitive.mPrimitiveId == PrimitiveId.Blend)
                list.Add(mPrimitive.toDataList());
            buf = new string[] { "IsShading", mBothShading.ToString() };
            list.Add(buf);
            buf = new string[] { "Disp3D", mDisp3D.ToString() };
            list.Add(buf);
            buf = new string[] { "LayerSize", mLayerBit.Length.ToString() };
            list.Add(buf);
            List<string> strings = new List<string> { "DispLayerBit" };
            for (int i = 0; i < mLayerBit.Length; i++) {
                strings.Add(mLayerBit[i].ToString("X2"));
            }
            list.Add(strings.ToArray());
            buf = new string[] { "ElementEnd" };
            list.Add(buf);
            return list;
        }

        /// <summary>
        /// 文字列配列データを変換してElementデータに設定
        /// </summary>
        /// <param name="dataList">文字列配列リスト</param>
        /// <param name="sp">リスト開始位置</param>
        /// <returns>リスト終了位置</returns>
        public int setDataList(List<string[]> dataList, int sp, bool wireFrame, bool surfaceVertex)
        {
            while (sp < dataList.Count) {
                string[] buf = dataList[sp++];
                try {
                    if (buf[0] == "PrimitiveId") {
                        switch (buf[1]) {
                            case "Line":
                                LinePrimitive line = new LinePrimitive();
                                line.setPropertyList(buf);
                                buf = dataList[sp++];
                                line.setDataList(buf);
                                mPrimitive = line;
                                break;
                            case "Arc":
                                ArcPrimitive arc = new ArcPrimitive();
                                arc.setPropertyList(buf);
                                buf = dataList[sp++];
                                arc.setDataList(buf);
                                mPrimitive = arc;
                                break;
                            case "Polyline":
                                PolylinePrimitive polyline = new PolylinePrimitive();
                                polyline.setPropertyList(buf);
                                buf = dataList[sp++];
                                polyline.setDataList(buf);
                                polyline.mPolyline.squeeze();
                                mPrimitive = polyline;
                                break;
                            case "Polygon":
                                PolygonPrimitive polygon = new PolygonPrimitive();
                                polygon.setPropertyList(buf);
                                buf = dataList[sp++];
                                polygon.setDataList(buf);
                                polygon.mPolygon.squeeze();
                                mPrimitive = polygon;
                                break;
                            case "Extrusion":
                                ExtrusionPrimitive extrusion = new ExtrusionPrimitive();
                                extrusion.setPropertyList(buf);
                                buf = dataList[sp++];
                                extrusion.setDataList(buf);
                                mPrimitive = extrusion;
                                break;
                            case "Blend":
                                BlendPrimitive blend = new BlendPrimitive();
                                blend.setPropertyList(buf);
                                buf = dataList[sp++];
                                blend.setDataList(buf);
                                buf = dataList[sp++];
                                blend.setDataList(buf);
                                mPrimitive = blend;
                                break;
                            case "Revolution":
                                RevolutionPrimitive revolution = new RevolutionPrimitive();
                                revolution.setPropertyList(buf);
                                buf = dataList[sp++];
                                revolution.setDataList(buf);
                                mPrimitive = revolution;
                                break;
                            case "Sweep":
                                SweepPrimitive sweep = new SweepPrimitive();
                                sweep.setPropertyList(buf);
                                buf = dataList[sp++];
                                sweep.setDataList(buf);
                                mPrimitive = sweep;
                                break;
                            default:
                                sp++;
                                break;
                        }
                        if (mPrimitive != null) {
                            mPrimitive.mSurfaceVertex = surfaceVertex;
                            mPrimitive.mWireFrame = wireFrame;
                            mPrimitive.createSurfaceData();
                            mPrimitive.createVertexData();
                        }
                    } else if (buf[0] == "Name") {
                        mName = buf[1];
                    } else if (buf[0] == "IsShading") {
                        mBothShading = ylib.boolParse(buf[1]);
                    } else if (buf[0] == "Disp3D") {
                        mDisp3D = ylib.boolParse(buf[1]);
                    } else if (buf[0] == "LayerSize") {
                        int layerSize = ylib.intParse(buf[1]);
                    } else if (buf[0] == "DispLayerBit") {
                        //mDispLayerBit = new byte[mLayerSize / 8];
                        for (int i = 0; i < mLayerBit.Length && i < buf.Length - 1; i++) {
                            mLayerBit[i] = byte.Parse(buf[i + 1], NumberStyles.HexNumber);
                        }
                    } else if (buf[0] == "ElementEnd") {
                        break;
                    }
                } catch (Exception e) {
                    System.Diagnostics.Debug.WriteLine($"Element setDataList: {sp} : {e.Message}");
                }
            }
            if (mPrimitive != null)
                update3DData();
            return sp;
        }
    }
}
