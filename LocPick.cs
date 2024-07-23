using CoreLib;
using System.Windows;

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

    /// <summary>
    /// ロケイト・ピック処理
    /// </summary>
    public class LocPick
    {
        //  アプリキーによるロケイトメニュー
        private List<string> mLocMenu = new List<string>() {
            "座標入力", "相対座標入力"
        };
        //  Ctrl + マウス右ピックによるロケイトメニュー
        private List<string> mLocSelectMenu = new List<string>() {
            "端点・中間点", "3分割点", "4分割点", "5分割点", "6分割点", "8分割点",
            "垂点", "中心点",
        };

        public List<Point3D> mLocList = new();                      //  ロケイトの保存
        public List<PickData> mPickElement = new List<PickData>();  //  ピックエレメント

        public DataManage mDataManage;
        private Window mMainWindow;
        private YCalc ycalc = new YCalc();
        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataManage"></param>
        /// <param name="mainWindow"></param>
        public LocPick(DataManage dataManage, Window mainWindow)
        {
            mDataManage = dataManage;
            mMainWindow = mainWindow;
        }

        /// <summary>
        /// データクリア
        /// </summary>
        public void clear()
        {
            mLocList.Clear();
            mPickElement.Clear();
        }

        /// <summary>
        /// オートロケイト
        /// </summary>
        /// <param name="pickPos">ピック座標</param>
        /// <param name="picks">ピック要素</param>
        public void autoLoc(PointD pickPos, List<int> picks, FACE3D face)
        {
            Point3D? wp = null;
            if (ylib.onControlKey()) {
                //  Ctrlキーでのメニュー表示で位置を選定
                wp = locSelect(pickPos, picks, face);
            } else {
                //  ピックされているときは位置を自動判断
                if (picks.Count == 1) {
                    //  ピックされているときは位置を自動判断
                    wp = autoLoc(pickPos, face, picks[0]);
                } else if (2 <= picks.Count) {
                    //  2要素の時は交点位置
                    wp = intersectionLoc(picks[0], picks[1], pickPos, face);
                    if (wp == null || double.IsNaN(wp.x))
                        wp = autoLoc(pickPos, face, picks[0]);
                    if (wp == null || double.IsNaN(wp.x))
                        wp = autoLoc(pickPos, face, picks[1]);
                }
            }
            if (wp != null) {
                if (mDataManage.mBaseLoc)
                    mLocList.Add(new Point3D(wp.toPoint(face), face));
                else
                    mLocList.Add(wp);
            }
        }

        /// <summary>
        /// 4分割点で最も近い座標を選択
        /// </summary>
        /// <param name="pos">ピック座標</param>
        /// <param name="entNo">要素番号</param>
        /// <returns>座標</returns>
        private Point3D autoLoc(PointD pos, FACE3D face, int entNo = -1)
        {
            if (mDataManage.mElementList[entNo].mPrimitive.mPrimitiveId == PrimitiveId.Arc) {
                ArcPrimitive arcPrimitive = (ArcPrimitive)mDataManage.mElementList[entNo].mPrimitive;
                return arcPrimitive.mArc.nearPoint(pos, 4, face);
            } else if (mDataManage.mElementList[entNo].mPrimitive.mPrimitiveId == PrimitiveId.Polyline) {
                PolylinePrimitive polylinePrimitive = (PolylinePrimitive)mDataManage.mElementList[entNo].mPrimitive;
                return polylinePrimitive.mPolyline.nearPoint(pos, 4, face);
            } else if (mDataManage.mElementList[entNo].mPrimitive.mPrimitiveId == PrimitiveId.Polygon) {
                PolygonPrimitive polygonPrimitive = (PolygonPrimitive)mDataManage.mElementList[entNo].mPrimitive;
                return polygonPrimitive.mPolygon.nearPoint(pos, 4, face);
            } else
                return mDataManage.mElementList[entNo].mPrimitive.nearPoint(pos, 4, face);
        }

        /// <summary>
        /// ピックした要素の交点座標
        /// </summary>
        /// <param name="entNo0">ピック要素番号</param>
        /// <param name="entNo1">ピック要素番号</param>
        /// <param name="pos">ピック位置</param>
        /// <returns>交点座標</returns>
        private Point3D intersectionLoc(int entNo0, int entNo1, PointD pos, FACE3D face)
        {
            Primitive ent0 = mDataManage.mElementList[entNo0].mPrimitive;
            Primitive ent1 = mDataManage.mElementList[entNo1].mPrimitive;
            Line3D line0 = ent0.getLine(pos, face);
            Line3D line1 = ent1.getLine(pos, face);
            PointD ip = line0.toLineD(face).intersection(line1.toLineD(face));
            if (ip == null) return null;
            return line0.intersection(ip, face);
        }

        /// <summary>
        /// Ctrl + マウス右ピックによるロケイトメニューの表ぞ
        /// </summary>
        /// <param name="pos">ピック位置</param>
        /// <param name="picks">ピック要素</param>
        /// <returns>ロケイト座標</returns>
        private Point3D locSelect(PointD pos, List<int> picks, FACE3D face)
        {
            if (picks.Count == 0) return new Point3D(pos, face);
            List<string> locMenu = new();
            locMenu.AddRange(mLocSelectMenu);
            Primitive ent = mDataManage.mElementList[picks[0]].mPrimitive;
            if (picks.Count == 1) {
                if (ent.mPrimitiveId == PrimitiveId.Arc) {
                    locMenu.Add("頂点");
                    //locMenu.Add("接点");
                }
            } else if (1 < picks.Count) {
                locMenu.Add("交点");
            }
            MenuDialog dlg = new MenuDialog();
            dlg.Title = "ロケイトメニュー";
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.mMenuList = locMenu;
            dlg.ShowDialog();
            Point3D pos3 = new Point3D(pos, face);
            if (0 < dlg.mResultMenu.Length) {
                pos3 = getLocSelectPos(dlg.mResultMenu, pos, picks, face);
            }
            return pos3;
        }

        /// <summary>
        /// Ctrl + マウス右ピックによるロケイトメニューの選択後の処理
        /// </summary>
        /// <param name="selectMenu">選択された項目</param>
        /// <param name="pos">ピック位置</param>
        /// <param name="picks">ピック要素</param>
        /// <returns>選択座標</returns>
        private Point3D getLocSelectPos(string selectMenu, PointD pos, List<int> picks, FACE3D face)
        {
            Primitive ent = mDataManage.mElementList[picks[0]].mPrimitive;
            Point3D lastLoc = new Point3D(pos, face);
            Point3D pos3 = new Point3D(pos, face);
            if (0 < mLocList.Count)
                lastLoc = mLocList[mLocList.Count - 1];
            List<Point3D> plist = new List<Point3D>();
            switch (selectMenu) {
                case "端点・中間点": pos3 = ent.nearPoint(pos, 2, face); break;
                case "3分割点": pos3 = ent.nearPoint(pos, 3, face); break;
                case "4分割点": pos3 = ent.nearPoint(pos, 4, face); break;
                case "5分割点": pos3 = ent.nearPoint(pos, 5, face); break;
                case "6分割点": pos3 = ent.nearPoint(pos, 6, face); break;
                case "8分割点": pos3 = ent.nearPoint(pos, 8, face); break;
                case "垂点": pos3 = ent.nearPoint(lastLoc.toPoint(face), 0, face); break;
                //case "接点":
                //    if (ent.mPrimitiveId == PrimitiveId.Arc) {
                //        ArcPrimitive arcEnt = (ArcPrimitive)ent;
                //        plist = arcEnt.mArc.tangentPoint(lastLoc);
                //    }
                //    if (plist != null && 0 < plist.Count)
                //        pos = plist.MinBy(p => p.length(pos));  //  最短位置
                //    break;
                case "頂点":
                    if (ent.mPrimitiveId == PrimitiveId.Arc) {
                        ArcPrimitive arcEnt = (ArcPrimitive)ent;
                        plist = arcEnt.mArc.toPeackList();
                    }
                    if (plist != null && 0 < plist.Count)
                        pos3 = plist.MinBy(p => p.length(pos3));  //  最短位置(見直し要)
                    break;
                case "中心点":
                    if (ent.mPrimitiveId == PrimitiveId.Arc) {
                        ArcPrimitive arcEnt = (ArcPrimitive)ent;
                        pos3 = arcEnt.mArc.mCp;
                    } else {
                        pos3 = ent.getArea().getCenter();
                    }
                    break;
            }
            return pos3;
        }

        /// <summary>
        /// ロケイトメニューの表示(Windowsメニューキー)
        /// </summary>
        public void locMenu(OPERATION operation, OPEMODE locMode)
        {
            if (locMode == OPEMODE.loc) {
                List<string> locMenu = new List<string>();
                locMenu.AddRange(mLocMenu);
                if (operation == OPERATION.translate || operation == OPERATION.copyTranslate) {
                    locMenu.Add("平行距離");
                } else if (operation == OPERATION.offset || operation == OPERATION.copyOffset) {
                    locMenu.Add("平行距離");
                } else if (operation == OPERATION.circle || operation == OPERATION.arc) {
                    locMenu.Add("半径");
                } else if (operation == OPERATION.rotate || operation == OPERATION.copyRotate) {
                    locMenu.Add("回転角");
                }
                MenuDialog dlg = new MenuDialog();
                dlg.Title = "ロケイトメニュー";
                dlg.Owner = mMainWindow;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlg.mMenuList = locMenu;
                dlg.ShowDialog();
                if (0 < dlg.mResultMenu.Length) {
                    getInputLoc(dlg.mResultMenu, operation);
                }
            }
        }

        /// <summary>
        /// ロケイトメニューの処理(Windowsメニューキー)
        /// </summary>
        /// <param name="title"></param>
        private void getInputLoc(string title, OPERATION operation)
        {
            InputBox dlg = new InputBox();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.Title = title;
            if (dlg.ShowDialog() == true) {
                string[] valstr;
                double val;
                int repeat = 1;
                PointD wp = new PointD();
                Point3D wp3 = new Point3D();
                PointD p1, p2;
                LineD line;
                Point3D lastLoc = new Point3D();
                Primitive ent;
                if (0 < mLocList.Count)
                    lastLoc = mLocList[mLocList.Count - 1];
                switch (title) {
                    case "座標入力":
                        //  xxx,yyy で入力
                        valstr = dlg.mEditText.Split(',');
                        if (valstr.Length == 1) {
                            wp = new PointD(ycalc.expression(valstr[0]), 0);
                            mLocList.Add(new Point3D(wp, mDataManage.mFace));
                        } else if (valstr.Length == 2) {
                            wp = new PointD(ycalc.expression(valstr[0]), ycalc.expression(valstr[1]));
                            mLocList.Add(new Point3D(wp, mDataManage.mFace));
                        } else if (valstr.Length == 3) {
                            wp3 = new Point3D(ycalc.expression(valstr[0]), ycalc.expression(valstr[1]), ycalc.expression(valstr[2]));
                            mLocList.Add(new Point3D(wp, mDataManage.mFace));
                        }
                        break;
                    case "相対座標入力":
                        //  xxx,yyy で入力
                        valstr = dlg.mEditText.Split(',');
                        if (1 < valstr.Length && 0 < mLocList.Count) {
                            wp = new PointD(ycalc.expression(valstr[0]), ycalc.expression(valstr[1]));
                            wp3 = new Point3D(wp, mDataManage.mFace);
                            if (2 < valstr.Length)
                                repeat = (int)ycalc.expression(valstr[2]);
                            for (int i = 0; i < repeat; i++)
                                mLocList.Add(wp3 + mLocList.Last());
                        }
                        break;
                    case "平行距離":
                        //  移動またはコピー移動の時のみ
                        if (0 == mLocList.Count)     //  方向を決めるロケイトが必要
                            break;
                        valstr = dlg.mEditText.Split(',');
                        val = ycalc.expression(valstr[0]);
                        if (1 < valstr.Length)
                            repeat = (int)ycalc.expression(valstr[1]);
                        ent = mDataManage.mElementList[mPickElement[mPickElement.Count - 1].mElementNo].mPrimitive;
                        if (ent.mPrimitiveId == PrimitiveId.Arc) {
                            ArcPrimitive arcEnt = (ArcPrimitive)ent;
                            LineD la = new LineD(arcEnt.mArc.mCp.toPoint(mDataManage.mFace), lastLoc.toPoint(mDataManage.mFace));
                            for (int i = 1; i < repeat + 1; i++) {
                                la.setLength(la.length() + val);
                                wp3 = new Point3D(la.pe, mDataManage.mFace);
                                mLocList.Add(wp3);
                            }
                        } else {
                            line = ent.getLine(mPickElement[mPickElement.Count - 1].mPos, mDataManage.mFace).toLineD(mDataManage.mFace);
                            if (!line.isNaN()) {
                                for (int i = 1; i < repeat + 1; i++) {
                                    wp3 = new Point3D(line.offset(lastLoc.toPoint(mDataManage.mFace), val * i), mDataManage.mFace);
                                    mLocList.Add(wp3);
                                }
                            }
                        }
                        break;
                    case "半径":
                        valstr = dlg.mEditText.Split(',');
                        val = ycalc.expression(valstr[0]);
                        if (operation == OPERATION.circle) {
                            //  円の作成
                            wp = new PointD(val, 0);
                            wp3 = lastLoc + new Point3D(wp, mDataManage.mFace);
                        } else {
                            break;
                        }
                        if (!wp.isNaN()) {
                            mLocList.Add(wp3);
                        }
                        break;
                    case "回転角":
                        valstr = dlg.mEditText.Split(',');
                        val = ycalc.expression(valstr[0]);
                        if (1 < valstr.Length)
                            repeat = (int)ycalc.expression(valstr[1]);
                        PointD vec = new PointD(1, 0);
                        if (1 == mLocList.Count) {
                            wp = mLocList[0].toPoint(mDataManage.mFace) + vec;
                            mLocList.Add(new Point3D(wp, mDataManage.mFace));
                        }
                        for (int i =1; i < repeat + 1; i++) {
                            vec = mLocList[i].toPoint(mDataManage.mFace) - mLocList[0].toPoint(mDataManage.mFace);
                            vec.rotate(ylib.D2R(val));
                            wp = mLocList[0].toPoint(mDataManage.mFace) + vec;
                            mLocList.Add(new Point3D(wp, mDataManage.mFace));
                        }
                        break;
                }
            }
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
            return mDataManage.findIndex(b, mDataManage.mFace);
        }

        /// <summary>
        /// 領域指定のピック処理
        /// </summary>
        /// <param name="pickArea">ピック領域</param>
        /// <returns>ピックリスト</returns>
        public List<int> getPickNo(Box pickArea)
        {
            return mDataManage.findIndex(pickArea, mDataManage.mFace);
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
                        addPickList(picks[i], wpos, mDataManage.mFace);
                } else {
                    int pickNo = pickSelect(picks);
                    if (0 <= pickNo) {
                        //  ピック要素の登録
                        addPickList(pickNo, wpos, mDataManage.mFace);
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
                Element ele = mDataManage.mElementList[sqeezePicks[i]];
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
        /// すでにピックされている場合はピックを解除
        /// </summary>
        /// <param name="pickNo">要素番号</param>
        /// <param name="pickPos">ピック位置</param>
        /// <param name="face">表示面</param>
        public void addPickList(int pickNo, PointD pickPos, FACE3D face)
        {
            int index = mPickElement.FindIndex(p => p.mElementNo == pickNo);
            if (0 <= index)
                mPickElement.RemoveAt(index);
            else
                mPickElement.Add(new PickData(pickNo, pickPos, face));
        }


        /// <summary>
        /// ピックフラグの設定
        /// </summary>
        public void setPick()
        {
            for (int i = 0; i < mPickElement.Count; i++)
                mDataManage.mElementList[mPickElement[i].mElementNo].mPrimitive.mPick = true;
        }

        /// <summary>
        /// ピックフラグの全解除
        /// </summary>
        public void pickReset()
        {
            for (int i = 0; i < mPickElement.Count; i++)
                mDataManage.mElementList[mPickElement[i].mElementNo].mPrimitive.mPick = false;
        }
    }
}
