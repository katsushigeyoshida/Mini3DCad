using CoreLib;
using System.Windows;

namespace Mini3DCad
{
    /// <summary>
    /// 操作モード
    /// </summary>
    public enum OPEMODE
    {
        non, pick, loc, areaDisp, areaPick, clear
    }

    /// <summary>
    /// 操作コマンドコード
    /// </summary>
    public enum OPERATION
    {
        non, loc, pick,
        point, line, circle, arc, rect, polyline, polygon,
        translate, rotate, offset, mirror, trim, strach, scale,
        copyTranslate, copyRotate, copyOffset, copyMirror, copyTrim, copyScale, copyElement, pasteElement,
        connect, divide, changeProperty, changePropertyAll,
        extrusion, revolution, sweep, release,
        measure, measureDistance, measureAngle,
        dispLayer, addLayer, removeLayer, info, remove, undo,
        screenCopy, screenSave,
        save, load, back, cancel, close
    }

    /// <summary>
    /// コマンドレベル
    /// </summary>
    public enum COMMANDLEVEL
    {
        non, main, sub
    }

    /// <summary>
    /// コマンド操作
    /// </summary>
    class Command
    {
        public string mainCommand;
        public string subCommand;
        public OPERATION operation;

        public Command(string main, string sub, OPERATION ope)
        {
            mainCommand = main;
            subCommand = sub;
            operation = ope;
        }
    }

    /// <summary>
    /// コマンドリスト
    /// </summary>
    class CommandData
    {
        public List<Command> mCommandData = new() {
            new Command("作成",       "点",           OPERATION.point),
            new Command("作成",       "線分",         OPERATION.line),
            new Command("作成",       "折線",         OPERATION.polyline),
            new Command("作成",       "円",           OPERATION.circle),
            new Command("作成",       "円弧",         OPERATION.arc),
            new Command("作成",       "四角",         OPERATION.rect),
            new Command("作成",       "ポリゴン",     OPERATION.polygon),
            new Command("作成",       "戻る",         OPERATION.back),
            new Command("2D編集",     "移動",         OPERATION.translate),
            new Command("2D編集",     "回転",         OPERATION.rotate),
            new Command("2D編集",     "オフセット",   OPERATION.offset),
            new Command("2D編集",     "反転",         OPERATION.mirror),
            new Command("2D編集",     "トリム",       OPERATION.trim),
            new Command("2D編集",     "拡大縮小",     OPERATION.scale),
            new Command("2D編集",     "分割",         OPERATION.divide),
            new Command("2D編集",     "接続",         OPERATION.connect),
            new Command("2D編集",     "属性変更",     OPERATION.changeProperty),
            new Command("2D編集",     "一括属性変更", OPERATION.changePropertyAll),
            new Command("2D編集",     "戻る",         OPERATION.back),
            new Command("2Dコピー",   "移動",         OPERATION.copyTranslate),
            new Command("2Dコピー",   "回転",         OPERATION.copyRotate),
            new Command("2Dコピー",   "オフセット",   OPERATION.copyOffset),
            new Command("2Dコピー",   "反転",         OPERATION.copyMirror),
            new Command("2Dコピー",   "トリム",       OPERATION.copyTrim),
            new Command("2Dコピー",   "拡大縮小",     OPERATION.copyScale),
            new Command("2Dコピー",   "要素コピー",   OPERATION.copyElement),
            new Command("2Dコピー",   "要素貼付け",   OPERATION.pasteElement),
            new Command("2Dコピー",   "戻る",         OPERATION.back),
            new Command("3D編集",     "押出",         OPERATION.extrusion),
            new Command("3D編集",     "回転体",       OPERATION.revolution),
            new Command("3D編集",     "掃引",         OPERATION.sweep),
            new Command("3D編集",     "解除",         OPERATION.release),
            new Command("3D編集",     "戻る",         OPERATION.back),
            new Command("設定",       "表示レイヤ",   OPERATION.dispLayer),
            //new Command("設定",       "レイヤ追加",   OPERATION.addLayer),
            //new Command("設定",       "レイヤ削除",   OPERATION.removeLayer),
            new Command("設定",       "戻る",         OPERATION.back),
            new Command("計測",       "距離",         OPERATION.measureDistance),
            new Command("計測",       "角度",         OPERATION.measureAngle),
            new Command("計測",       "距離・角度",   OPERATION.measure),
            new Command("計測",       "戻る",         OPERATION.back),
            new Command("情報",       "情報",         OPERATION.info),
            new Command("削除",       "削除",         OPERATION.remove),
            new Command("アンドゥ",   "アンドゥ",     OPERATION.undo),
            //new Command("ファイル", "保存",         OPERATION.save),
            //new Command("ファイル", "読込",         OPERATION.load),
            new Command("ツール",     "画面コピー",   OPERATION.screenCopy),
            new Command("ツール",     "画面保存",     OPERATION.screenSave),
            new Command("ツール",     "戻る",         OPERATION.back),
            new Command("キャンセル", "キャンセル",   OPERATION.cancel),
            new Command("終了",       "終了",         OPERATION.close),
        };
        private string mMainCommand = "";

        /// <summary>
        /// メインコマンドリストの取得
        /// </summary>
        /// <returns>コマンドリスト</returns>
        public List<string> getMainCommand()
        {
            mMainCommand = "";
            List<string> main = new List<string>();
            foreach (var cmd in mCommandData) {
                if (!main.Contains(cmd.mainCommand) && cmd.mainCommand != "")
                    main.Add(cmd.mainCommand);
            }
            return main;
        }

        /// <summary>
        /// サブコマンドリストの取得
        /// </summary>
        /// <param name="main">メインコマンド</param>
        /// <returns>コマンドリスト</returns>
        public List<string> getSubCommand(string main)
        {
            mMainCommand = main;
            List<string> sub = new List<string>();
            foreach (var cmd in mCommandData) {
                if (cmd.mainCommand == main || cmd.mainCommand == "") {
                    if (!sub.Contains(cmd.subCommand))
                        sub.Add(cmd.subCommand);
                }
            }
            return sub;
        }

        /// <summary>
        /// コマンドレベルの取得
        /// </summary>
        /// <param name="menu">コマンド名</param>
        /// <returns>コマンドレベル</returns>
        public COMMANDLEVEL getCommandLevl(string menu)
        {
            int n = mCommandData.FindIndex(p => p.subCommand == menu);
            if (0 <= n)
                return COMMANDLEVEL.sub;
            n = mCommandData.FindIndex(p => p.mainCommand == menu);
            if (0 <= n)
                return COMMANDLEVEL.main;
            return COMMANDLEVEL.non;
        }

        /// <summary>
        /// コマンドコードの取得
        /// </summary>
        /// <param name="menu">サブコマンド名</param>
        /// <returns>コマンドコード</returns>
        public OPERATION getCommand(string menu)
        {
            if (0 <= mCommandData.FindIndex(p => (mMainCommand == "" || p.mainCommand == mMainCommand) && p.subCommand == menu)) {
                Command com = mCommandData.Find(p => (mMainCommand == "" || p.mainCommand == mMainCommand) && p.subCommand == menu);
                return com.operation;
            }
            return OPERATION.non;
        }
    }

    /// <summary>
    /// コマンド処理
    /// </summary>
    class CommandOpe
    {
        public int mSaveOperationCount = 10;                    //  定期保存の操作回数

        public KeyCommand mKeyCommand;                          //  キー入力コマンド
        public DataManage mDataManage;
        public OPERATION mOperation = OPERATION.non;
        public FACE3D mDispMode = FACE3D.XY;
        public string mDataFilePath = "";
        public ChkListDialog mLayerChkListDlg = null;           //  表示レイヤー設定ダイヤログ
        public MainWindow mMainWindow;
        private YLib ylib = new YLib();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mainWindow">MainWindow</param>
        /// <param name="dataManage">DataManage</param>
        public CommandOpe(MainWindow mainWindow, DataManage dataManage)
        {
            mMainWindow = mainWindow;
            mDataManage = dataManage;
            mKeyCommand = new KeyCommand(mDataManage);
        }

        /// <summary>
        /// コマンド実行
        /// </summary>
        /// <param name="ope">Operationコード</param>
        /// <param name="picks">ピックリスト</param>
        /// <returns></returns>
        public OPEMODE execCommand(OPERATION ope, List<PickData> picks)
        {
            mDataManage.mOperationCount++;
            mOperation = ope;
            OPEMODE opeMode = OPEMODE.loc;
            switch (ope) {
                case OPERATION.point: break;
                case OPERATION.line: break;
                case OPERATION.circle: break;
                case OPERATION.arc: break;
                case OPERATION.polyline: break;
                case OPERATION.rect: break;
                case OPERATION.polygon: break;
                case OPERATION.translate: break;
                case OPERATION.rotate: break;
                case OPERATION.offset: break;
                case OPERATION.mirror: break;
                case OPERATION.trim: break;
                case OPERATION.scale: break;
                case OPERATION.copyTranslate: break;
                case OPERATION.copyRotate: break;
                case OPERATION.copyOffset: break;
                case OPERATION.copyMirror: break;
                case OPERATION.copyTrim: break;
                case OPERATION.copyScale: break;
                case OPERATION.divide: break;
                case OPERATION.connect:
                    mDataManage.connect(picks);
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.changeProperty:
                    mDataManage.changeProperty(picks);
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.changePropertyAll:
                    mDataManage.changePropertyAll(picks);
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.copyElement:
                    mDataManage.copyElement(picks);
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.pasteElement:
                    mDataManage.getPasteElement();
                    break;
                case OPERATION.extrusion: break;
                case OPERATION.revolution:
                    if (2 == picks.Count)
                        mDataManage.revolution(picks);
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.sweep:
                    if (2 == picks.Count)
                        mDataManage.sweep(picks);
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.release:
                    mDataManage.release(picks);
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.dispLayer:
                    setDispLayer();
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.addLayer:
                    addLayer(); ;
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.measureAngle:
                    break;
                case OPERATION.measureDistance:
                    break;
                case OPERATION.measure:
                    mDataManage.measure(picks);
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.info:
                    mDataManage.info(picks);
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.remove:
                    mDataManage.remove(picks);
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.undo:
                    mDataManage.undo();
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.screenCopy:
                    mMainWindow.screenCopy();
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.screenSave:
                    mMainWindow.screenSave();
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.back:
                    opeMode = OPEMODE.non;
                    break;
                case OPERATION.save:
                    saveFile();
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.cancel:
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.close:
                    opeMode = OPEMODE.non;
                    mMainWindow.Close();
                    break;
                default: opeMode = OPEMODE.non; break;
            }
            if (opeMode != OPEMODE.loc)
                mDataManage.updateData();
            if (mDataManage.mOperationCount % mSaveOperationCount == 0)
                saveFile();
            mMainWindow.dispTitle();
            return opeMode;
        }

        /// <summary>
        /// キー入力によるコマンド処理
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <returns></returns>
        public bool keyCommand(string command)
        {
            mDataManage.mOperationCount++;
            return mKeyCommand.execCommand(command);
        }

        /// <summary>
        /// キーコマンドをファイルに保存
        /// </summary>
        public void saveKeycommnad()
        {
            mKeyCommand.saveFile();
        }

        /// <summary>
        /// 表示レイヤの設定
        /// </summary>
        public void setDispLayer()
        {
            if (mLayerChkListDlg != null)
                mLayerChkListDlg.Close();
            mLayerChkListDlg = new ChkListDialog();
            mLayerChkListDlg.Topmost = true;
            mLayerChkListDlg.mTitle = "表示レイヤー";
            mLayerChkListDlg.mAddMenuEnable    = true;
            mLayerChkListDlg.mEditMenuEnable   = true;
            mLayerChkListDlg.mDeleteMenuEnable = true;
            mLayerChkListDlg.mLayerAllEnable   = true;
            mLayerChkListDlg.mChkList  = mDataManage.mLayer.getLayerChkList();
            mLayerChkListDlg.mLayerAll = mDataManage.mLayer.mLayerAll;
            mLayerChkListDlg.mCallBackOn    = true;
            mLayerChkListDlg.callback       = setLayerChk;
            mLayerChkListDlg.callbackRename = layerRename;
            mLayerChkListDlg.Show();
            mDataManage.mOperationCount++;
        }

        /// <summary>
        /// レイヤーチェックリストに表示を更新(コールバック)
        /// </summary>
        public void setLayerChk()
        {
            mDataManage.mLayer.setLayerChkList(mLayerChkListDlg.mChkList);
            mDataManage.mLayer.mLayerAll = mLayerChkListDlg.mLayerAll;
            if (mDataManage.mFace == FACE3D.NON)
                mMainWindow.renderFrame();
            else
                mMainWindow.mDraw.draw(true);
            mMainWindow.dispTitle();
        }

        /// <summary>
        /// レイヤ名の変更(コールバック)
        /// </summary>
        public void layerRename()
        {
            mDataManage.mLayer.rename(mLayerChkListDlg.mSrcName, mLayerChkListDlg.mDestName);
            setDispLayer();
        }

        /// <summary>
        /// レイヤーの追加
        /// </summary>
        public void addLayer()
        {
            InputBox dlg = new InputBox();
            dlg.Owner = mMainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (dlg.ShowDialog() == true) {
                mDataManage.mLayer.add(dlg.mEditText);
            }
        }

        /// <summary>
        /// 全データ削除
        /// </summary>
        public void newData(bool unmsg = false)
        {
            if (unmsg || ylib.messageBox(mMainWindow, "すべてのデータを削除します。", "", "確認", MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes) {
                mDataManage.clear();
            }
        }

        /// <summary>
        /// ファイルにデータを保存
        /// </summary>
        /// <param name="saveonly">未使用</param>
        public void saveFile(bool saveonly = false)
        {
            if (0 < mDataFilePath.Length) {
                mDataManage.saveData(mDataFilePath);
            } else if (0 < mDataManage.mElementList.Count) {
                string itemName = mMainWindow.mFileData.addItem();
                if (0 < itemName.Length) {
                    mDataFilePath = mMainWindow.mFileData.getItemFilePath(itemName);
                    mDataManage.saveData(mDataFilePath);
                }
            }
        }

        /// <summary>
        /// ファイルデータの読込
        /// </summary>
        public void loadFile()
        {
            mDataManage.loadData(mDataFilePath);
        }
    }

}
