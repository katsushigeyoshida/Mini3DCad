﻿using CoreLib;
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
        line, circle, arc, rect, polyline, polygon,
        translate, rotate,
        copyTranslate, copyRotate,
        connect, divide, changeProperty,
        extrusion, revolution, release,
        info, remove, undo,
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
            new Command("作成",       "線分",       OPERATION.line),
            new Command("作成",       "折線",       OPERATION.polyline),
            new Command("作成",       "円",         OPERATION.circle),
            new Command("作成",       "円弧",       OPERATION.arc),
            new Command("作成",       "四角",       OPERATION.rect),
            new Command("作成",       "ポリゴン",   OPERATION.polygon),
            new Command("作成",       "戻る",       OPERATION.back),
            new Command("2D編集",     "移動",       OPERATION.translate),
            new Command("2D編集",     "回転",       OPERATION.rotate),
            new Command("2D編集",     "分割",       OPERATION.divide),
            new Command("2D編集",     "接続",       OPERATION.connect),
            new Command("2D編集",     "属性変更",   OPERATION.changeProperty),
            new Command("2D編集",     "戻る",       OPERATION.back),
            new Command("2Dコピー",   "移動",       OPERATION.copyTranslate),
            new Command("2Dコピー",   "回転",       OPERATION.copyRotate),
            new Command("2Dコピー",   "戻る",       OPERATION.back),
            new Command("3D編集",     "押出",       OPERATION.extrusion),
            new Command("3D編集",     "回転体",     OPERATION.revolution),
            new Command("3D編集",     "解除",       OPERATION.release),
            new Command("3D編集",     "戻る",       OPERATION.back),
            new Command("情報",       "情報",       OPERATION.info),
            new Command("削除",       "削除",       OPERATION.remove),
            new Command("アンドゥ",   "アンドゥ",   OPERATION.undo),
            //new Command("ファイル", "保存",         OPERATION.save),
            //new Command("ファイル", "読込",         OPERATION.load),
            //new Command("ファイル", "戻る",         OPERATION.back),
            new Command("キャンセル", "キャンセル", OPERATION.cancel),
            new Command("終了",       "終了",       OPERATION.close),
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
        public DataManage mDataManage;
        public OPERATION mOperation = OPERATION.non;
        public FACE3D mDispMode = FACE3D.XY;
        public string mDataFilePath = "dataFile.csv";
        public MainWindow mMainWindow;

        private YLib ylib = new YLib();

        public CommandOpe(MainWindow mainWindow, DataManage dataManage)
        {
            mMainWindow = mainWindow;
            mDataManage = dataManage;
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
                case OPERATION.line: break;
                case OPERATION.circle: break;
                case OPERATION.arc: break;
                case OPERATION.polyline: break;
                case OPERATION.rect: break;
                case OPERATION.polygon: break;
                case OPERATION.translate: break;
                case OPERATION.rotate: break;
                case OPERATION.copyTranslate: break;
                case OPERATION.copyRotate: break;
                case OPERATION.divide: break;
                case OPERATION.connect:
                    mDataManage.connect(picks);
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.changeProperty:
                    mDataManage.changeProperty(picks);
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.extrusion: break;
                case OPERATION.revolution:
                    if (1 < picks.Count)
                        mDataManage.revolution(picks);
                    opeMode = OPEMODE.clear;
                    break;
                case OPERATION.release:
                    mDataManage.release(picks);
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
                case OPERATION.back:
                    opeMode = OPEMODE.non;
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
            return opeMode;
        }

        /// <summary>
        /// キー入力によるコマンド処理
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <returns></returns>
        public bool keyCommand(string command)
        {
            //mEntityData.mOperationCouunt++;
            //mKeyCommand.mTextString = text;
            //return mKeyCommand.setCommand(command, mEntityData.mPara);
            return false;
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
        /// <param name="saveonly"></param>
        public void saveFile(bool saveonly = false)
        {
            if (0 < mDataFilePath.Length)
                mDataManage.saveData(mDataFilePath);
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
