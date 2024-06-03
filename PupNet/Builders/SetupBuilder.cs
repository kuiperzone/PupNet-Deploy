// -----------------------------------------------------------------------------
// PROJECT   : PupNet
// COPYRIGHT : Andy Thomas (C) 2022-24
// LICENSE   : GPL-3.0-or-later
// HOMEPAGE  : https://github.com/kuiperzone/PupNet
//
// PupNet is free software: you can redistribute it and/or modify it under
// the terms of the GNU Affero General Public License as published by the Free Software
// Foundation, either version 3 of the License, or (at your option) any later version.
//
// PupNet is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License along
// with PupNet. If not, see <https://www.gnu.org/licenses/>.
// -----------------------------------------------------------------------------

using System.Text;

namespace KuiperZone.PupNet.Builders;

/// <summary>
/// Extends <see cref="PackageBuilder"/> for Windows Setup package.
/// Leverages InnoSetup. Application installed into user space.
/// </summary>
public class SetupBuilder : PackageBuilder
{
    private const string PromptBat = "CommandPrompt.bat";

    /// <summary>
    /// Constructor.
    /// </summary>
    public SetupBuilder(ConfigurationReader conf)
        : base(conf, PackageKind.Setup)
    {
        BuildAppBin = Path.Combine(BuildRoot, "Publish");

        // Not used. This is decided at install time via interaction with user.
        InstallBin = "";

        ManifestBuildPath = Path.Combine(Root, Configuration.AppBaseName + ".iss");
        ManifestContent = GetInnoFile();

        var list = new List<string>();
        list.Add($"iscc /O\"{OutputDirectory}\" \"{ManifestBuildPath}\"");
        PackageCommands = list;
    }

    /// <summary>
    /// Gets terminal windows icon.
    /// </summary>
    public static string TerminalIcon { get; } = Path.Combine(AssemblyDirectory, "terminal.ico");

    /// <summary>
    /// Implements.
    /// </summary>
    public override string OutputName
    {
        get { return GetOutputName(Configuration.SetupVersionOutput, Configuration.SetupSuffixOutput, Architecture, ".exe"); }
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string Architecture
    {
        get
        {
            if (Arguments.Arch != null)
            {
                return Arguments.Arch;
            }

            // Where supported, these seem to match the Architecture enum names.
            // https://jrsoftware.org/ishelp/index.php?topic=setup_architecturesallowed
            return Runtime.RuntimeArch.ToString().ToLowerInvariant();
        }
    }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string BuildAppBin { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string InstallBin { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string? ManifestBuildPath { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override string? ManifestContent { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override IReadOnlyCollection<string> PackageCommands { get; }

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool SupportsStartCommand { get; } = true;

    /// <summary>
    /// Implements.
    /// </summary>
    public override bool SupportsPostRun { get; } = false;

    /// <summary>
    /// Overrides and extends.
    /// </summary>
    public override void Create(string? desktop, string? metainfo)
    {
        base.Create(desktop, metainfo);

                //Create zh-cn language file
        string zhcnLanguagePath = Path.Combine(Root, "zhcn.isl");
        Operations.WriteFile(zhcnLanguagePath, "; *** Inno Setup version 6.1.0+ Chinese Simplified messages ***\r\n;\r\n; To download user-contributed translations of this file, go to:\r\n;   https://jrsoftware.org/files/istrans/\r\n;\r\n; Note: When translating this text, do not add periods (.) to the end of\r\n; messages that didn't have them already, because on those messages Inno\r\n; Setup adds the periods automatically (appending a period would result in\r\n; two periods being displayed).\r\n;\r\n; Maintained by Zhenghan Yang\r\n; Email: 847320916@QQ.com\r\n; Translation based on network resource\r\n; The latest Translation is on https://github.com/kira-96/Inno-Setup-Chinese-Simplified-Translation\r\n;\r\n\r\n[LangOptions]\r\n; The following three entries are very important. Be sure to read and \r\n; understand the '[LangOptions] section' topic in the help file.\r\nLanguageName=简体中文\r\n; If Language Name display incorrect, uncomment next line\r\n; LanguageName=<7B80><4F53><4E2D><6587>\r\n; About LanguageID, to reference link:\r\n; https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c\r\nLanguageID=$0804\r\n; About CodePage, to reference link:\r\n; https://docs.microsoft.com/en-us/windows/win32/intl/code-page-identifiers\r\nLanguageCodePage=936\r\n; If the language you are translating to requires special font faces or\r\n; sizes, uncomment any of the following entries and change them accordingly.\r\n;DialogFontName=\r\n;DialogFontSize=8\r\n;WelcomeFontName=Verdana\r\n;WelcomeFontSize=12\r\n;TitleFontName=Arial\r\n;TitleFontSize=29\r\n;CopyrightFontName=Arial\r\n;CopyrightFontSize=8\r\n\r\n[Messages]\r\n\r\n; *** 应用程序标题\r\nSetupAppTitle=安装\r\nSetupWindowTitle=安装 - %1\r\nUninstallAppTitle=卸载\r\nUninstallAppFullTitle=%1 卸载\r\n\r\n; *** Misc. common\r\nInformationTitle=信息\r\nConfirmTitle=确认\r\nErrorTitle=错误\r\n\r\n; *** SetupLdr messages\r\nSetupLdrStartupMessage=现在将安装 %1。您想要继续吗？\r\nLdrCannotCreateTemp=无法创建临时文件。安装程序已中止\r\nLdrCannotExecTemp=无法执行临时目录中的文件。安装程序已中止\r\nHelpTextNote=\r\n\r\n; *** 启动错误消息\r\nLastErrorMessage=%1。%n%n错误 %2: %3\r\nSetupFileMissing=安装目录中缺少文件 %1。请修正这个问题或者获取程序的新副本。\r\nSetupFileCorrupt=安装文件已损坏。请获取程序的新副本。\r\nSetupFileCorruptOrWrongVer=安装文件已损坏，或是与这个安装程序的版本不兼容。请修正这个问题或获取新的程序副本。\r\nInvalidParameter=无效的命令行参数：%n%n%1\r\nSetupAlreadyRunning=安装程序正在运行。\r\nWindowsVersionNotSupported=此程序不支持当前计算机运行的 Windows 版本。\r\nWindowsServicePackRequired=此程序需要 %1 服务包 %2 或更高版本。\r\nNotOnThisPlatform=此程序不能在 %1 上运行。\r\nOnlyOnThisPlatform=此程序只能在 %1 上运行。\r\nOnlyOnTheseArchitectures=此程序只能在为下列处理器结构设计的 Windows 版本中安装：%n%n%1\r\nWinVersionTooLowError=此程序需要 %1 版本 %2 或更高。\r\nWinVersionTooHighError=此程序不能安装于 %1 版本 %2 或更高。\r\nAdminPrivilegesRequired=在安装此程序时您必须以管理员身份登录。\r\nPowerUserPrivilegesRequired=在安装此程序时您必须以管理员身份或有权限的用户组身份登录。\r\nSetupAppRunningError=安装程序发现 %1 当前正在运行。%n%n请先关闭正在运行的程序，然后点击“确定”继续，或点击“取消”退出。\r\nUninstallAppRunningError=卸载程序发现 %1 当前正在运行。%n%n请先关闭正在运行的程序，然后点击“确定”继续，或点击“取消”退出。\r\n\r\n; *** 启动问题\r\nPrivilegesRequiredOverrideTitle=选择安装程序模式\r\nPrivilegesRequiredOverrideInstruction=选择安装模式\r\nPrivilegesRequiredOverrideText1=%1 可以为所有用户安装(需要管理员权限)，或仅为您安装。\r\nPrivilegesRequiredOverrideText2=%1 只能为您安装，或为所有用户安装(需要管理员权限)。\r\nPrivilegesRequiredOverrideAllUsers=为所有用户安装(&A)\r\nPrivilegesRequiredOverrideAllUsersRecommended=为所有用户安装(&A) (建议选项)\r\nPrivilegesRequiredOverrideCurrentUser=只为我安装(&M)\r\nPrivilegesRequiredOverrideCurrentUserRecommended=只为我安装(&M) (建议选项)\r\n\r\n; *** 其他错误\r\nErrorCreatingDir=安装程序无法创建目录“%1”。\r\nErrorTooManyFilesInDir=无法在目录“%1”中创建文件，因为里面包含太多文件\r\n\r\n; *** 安装程序公共消息\r\nExitSetupTitle=退出安装程序\r\nExitSetupMessage=安装程序尚未完成。如果现在退出，将不会安装该程序。%n%n您之后可以再次运行安装程序完成安装。%n%n现在退出安装程序吗？\r\nAboutSetupMenuItem=关于安装程序(&A)...\r\nAboutSetupTitle=关于安装程序\r\nAboutSetupMessage=%1 版本 %2%n%3%n%n%1 主页：%n%4\r\nAboutSetupNote=\r\nTranslatorNote=简体中文翻译由Kira(847320916@qq.com)维护。项目地址：https://github.com/kira-96/Inno-Setup-Chinese-Simplified-Translation\r\n\r\n; *** 按钮\r\nButtonBack=< 上一步(&B)\r\nButtonNext=下一步(&N) >\r\nButtonInstall=安装(&I)\r\nButtonOK=确定\r\nButtonCancel=取消\r\nButtonYes=是(&Y)\r\nButtonYesToAll=全是(&A)\r\nButtonNo=否(&N)\r\nButtonNoToAll=全否(&O)\r\nButtonFinish=完成(&F)\r\nButtonBrowse=浏览(&B)...\r\nButtonWizardBrowse=浏览(&R)...\r\nButtonNewFolder=新建文件夹(&M)\r\n\r\n; *** “选择语言”对话框消息\r\nSelectLanguageTitle=选择安装语言\r\nSelectLanguageLabel=选择安装时使用的语言。\r\n\r\n; *** 公共向导文字\r\nClickNext=点击“下一步”继续，或点击“取消”退出安装程序。\r\nBeveledLabel=\r\nBrowseDialogTitle=浏览文件夹\r\nBrowseDialogLabel=在下面的列表中选择一个文件夹，然后点击“确定”。\r\nNewFolderName=新建文件夹\r\n\r\n; *** “欢迎”向导页\r\nWelcomeLabel1=欢迎使用 [name] 安装向导\r\nWelcomeLabel2=现在将安装 [name/ver] 到您的电脑中。%n%n建议您在继续安装前关闭所有其他应用程序。\r\n\r\n; *** “密码”向导页\r\nWizardPassword=密码\r\nPasswordLabel1=这个安装程序有密码保护。\r\nPasswordLabel3=请输入密码，然后点击“下一步”继续。密码区分大小写。\r\nPasswordEditLabel=密码(&P)：\r\nIncorrectPassword=您输入的密码不正确，请重新输入。\r\n\r\n; *** “许可协议”向导页\r\nWizardLicense=许可协议\r\nLicenseLabel=请在继续安装前阅读以下重要信息。\r\nLicenseLabel3=请仔细阅读下列许可协议。在继续安装前您必须同意这些协议条款。\r\nLicenseAccepted=我同意此协议(&A)\r\nLicenseNotAccepted=我不同意此协议(&D)\r\n\r\n; *** “信息”向导页\r\nWizardInfoBefore=信息\r\nInfoBeforeLabel=请在继续安装前阅读以下重要信息。\r\nInfoBeforeClickLabel=准备好继续安装后，点击“下一步”。\r\nWizardInfoAfter=信息\r\nInfoAfterLabel=请在继续安装前阅读以下重要信息。\r\nInfoAfterClickLabel=准备好继续安装后，点击“下一步”。\r\n\r\n; *** “用户信息”向导页\r\nWizardUserInfo=用户信息\r\nUserInfoDesc=请输入您的信息。\r\nUserInfoName=用户名(&U)：\r\nUserInfoOrg=组织(&O)：\r\nUserInfoSerial=序列号(&S)：\r\nUserInfoNameRequired=您必须输入用户名。\r\n\r\n; *** “选择目标目录”向导页\r\nWizardSelectDir=选择目标位置\r\nSelectDirDesc=您想将 [name] 安装在哪里？\r\nSelectDirLabel3=安装程序将安装 [name] 到下面的文件夹中。\r\nSelectDirBrowseLabel=点击“下一步”继续。如果您想选择其他文件夹，点击“浏览”。\r\nDiskSpaceGBLabel=至少需要有 [gb] GB 的可用磁盘空间。\r\nDiskSpaceMBLabel=至少需要有 [mb] MB 的可用磁盘空间。\r\nCannotInstallToNetworkDrive=安装程序无法安装到一个网络驱动器。\r\nCannotInstallToUNCPath=安装程序无法安装到一个 UNC 路径。\r\nInvalidPath=您必须输入一个带驱动器卷标的完整路径，例如：%n%nC:\\APP%n%n或UNC路径：%n%n\\\\server\\share\r\nInvalidDrive=您选定的驱动器或 UNC 共享不存在或不能访问。请选择其他位置。\r\nDiskSpaceWarningTitle=磁盘空间不足\r\nDiskSpaceWarning=安装程序至少需要 %1 KB 的可用空间才能安装，但选定驱动器只有 %2 KB 的可用空间。%n%n您一定要继续吗？\r\nDirNameTooLong=文件夹名称或路径太长。\r\nInvalidDirName=文件夹名称无效。\r\nBadDirName32=文件夹名称不能包含下列任何字符：%n%n%1\r\nDirExistsTitle=文件夹已存在\r\nDirExists=文件夹：%n%n%1%n%n已经存在。您一定要安装到这个文件夹中吗？\r\nDirDoesntExistTitle=文件夹不存在\r\nDirDoesntExist=文件夹：%n%n%1%n%n不存在。您想要创建此文件夹吗？\r\n\r\n; *** “选择组件”向导页\r\nWizardSelectComponents=选择组件\r\nSelectComponentsDesc=您想安装哪些程序组件？\r\nSelectComponentsLabel2=选中您想安装的组件；取消您不想安装的组件。然后点击“下一步”继续。\r\nFullInstallation=完全安装\r\n; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)\r\nCompactInstallation=简洁安装\r\nCustomInstallation=自定义安装\r\nNoUninstallWarningTitle=组件已存在\r\nNoUninstallWarning=安装程序检测到下列组件已安装在您的电脑中：%n%n%1%n%n取消选中这些组件不会卸载它们。%n%n确定要继续吗？\r\nComponentSize1=%1 KB\r\nComponentSize2=%1 MB\r\nComponentsDiskSpaceGBLabel=当前选择的组件需要至少 [gb] GB 的磁盘空间。\r\nComponentsDiskSpaceMBLabel=当前选择的组件需要至少 [mb] MB 的磁盘空间。\r\n\r\n; *** “选择附加任务”向导页\r\nWizardSelectTasks=选择附加任务\r\nSelectTasksDesc=您想要安装程序执行哪些附加任务？\r\nSelectTasksLabel2=选择您想要安装程序在安装 [name] 时执行的附加任务，然后点击“下一步”。\r\n\r\n; *** “选择开始菜单文件夹”向导页\r\nWizardSelectProgramGroup=选择开始菜单文件夹\r\nSelectStartMenuFolderDesc=安装程序应该在哪里放置程序的快捷方式？\r\nSelectStartMenuFolderLabel3=安装程序将在下列“开始”菜单文件夹中创建程序的快捷方式。\r\nSelectStartMenuFolderBrowseLabel=点击“下一步”继续。如果您想选择其他文件夹，点击“浏览”。\r\nMustEnterGroupName=您必须输入一个文件夹名。\r\nGroupNameTooLong=文件夹名或路径太长。\r\nInvalidGroupName=无效的文件夹名字。\r\nBadGroupName=文件夹名不能包含下列任何字符：%n%n%1\r\nNoProgramGroupCheck2=不创建开始菜单文件夹(&D)\r\n\r\n; *** “准备安装”向导页\r\nWizardReady=准备安装\r\nReadyLabel1=安装程序准备就绪，现在可以开始安装 [name] 到您的电脑。\r\nReadyLabel2a=点击“安装”继续此安装程序。如果您想重新考虑或修改任何设置，点击“上一步”。\r\nReadyLabel2b=点击“安装”继续此安装程序。\r\nReadyMemoUserInfo=用户信息：\r\nReadyMemoDir=目标位置：\r\nReadyMemoType=安装类型：\r\nReadyMemoComponents=已选择组件：\r\nReadyMemoGroup=开始菜单文件夹：\r\nReadyMemoTasks=附加任务：\r\n\r\n; *** TDownloadWizardPage wizard page and DownloadTemporaryFile\r\nDownloadingLabel=正在下载附加文件...\r\nButtonStopDownload=停止下载(&S)\r\nStopDownload=您确定要停止下载吗？\r\nErrorDownloadAborted=下载已中止\r\nErrorDownloadFailed=下载失败：%1 %2\r\nErrorDownloadSizeFailed=获取下载大小失败：%1 %2\r\nErrorFileHash1=校验文件哈希失败：%1\r\nErrorFileHash2=无效的文件哈希：预期 %1，实际 %2\r\nErrorProgress=无效的进度：%1 / %2\r\nErrorFileSize=文件大小错误：预期 %1，实际 %2\r\n\r\n; *** “正在准备安装”向导页\r\nWizardPreparing=正在准备安装\r\nPreparingDesc=安装程序正在准备安装 [name] 到您的电脑。\r\nPreviousInstallNotCompleted=先前的程序安装或卸载未完成，您需要重启您的电脑以完成。%n%n在重启电脑后，再次运行安装程序以完成 [name] 的安装。\r\nCannotContinue=安装程序不能继续。请点击“取消”退出。\r\nApplicationsFound=以下应用程序正在使用将由安装程序更新的文件。建议您允许安装程序自动关闭这些应用程序。\r\nApplicationsFound2=以下应用程序正在使用将由安装程序更新的文件。建议您允许安装程序自动关闭这些应用程序。安装完成后，安装程序将尝试重新启动这些应用程序。\r\nCloseApplications=自动关闭应用程序(&A)\r\nDontCloseApplications=不要关闭应用程序(&D)\r\nErrorCloseApplications=安装程序无法自动关闭所有应用程序。建议您在继续之前，关闭所有在使用需要由安装程序更新的文件的应用程序。\r\nPrepareToInstallNeedsRestart=安装程序必须重启您的计算机。计算机重启后，请再次运行安装程序以完成 [name] 的安装。%n%n是否立即重新启动？\r\n\r\n; *** “正在安装”向导页\r\nWizardInstalling=正在安装\r\nInstallingLabel=安装程序正在安装 [name] 到您的电脑，请稍候。\r\n\r\n; *** “安装完成”向导页\r\nFinishedHeadingLabel=[name] 安装完成\r\nFinishedLabelNoIcons=安装程序已在您的电脑中安装了 [name]。\r\nFinishedLabel=安装程序已在您的电脑中安装了 [name]。您可以通过已安装的快捷方式运行此应用程序。\r\nClickFinish=点击“完成”退出安装程序。\r\nFinishedRestartLabel=为完成 [name] 的安装，安装程序必须重新启动您的电脑。要立即重启吗？\r\nFinishedRestartMessage=为完成 [name] 的安装，安装程序必须重新启动您的电脑。%n%n要立即重启吗？\r\nShowReadmeCheck=是，我想查阅自述文件\r\nYesRadio=是，立即重启电脑(&Y)\r\nNoRadio=否，稍后重启电脑(&N)\r\n; used for example as 'Run MyProg.exe'\r\nRunEntryExec=运行 %1\r\n; used for example as 'View Readme.txt'\r\nRunEntryShellExec=查阅 %1\r\n\r\n; *** “安装程序需要下一张磁盘”提示\r\nChangeDiskTitle=安装程序需要下一张磁盘\r\nSelectDiskLabel2=请插入磁盘 %1 并点击“确定”。%n%n如果这个磁盘中的文件可以在下列文件夹之外的文件夹中找到，请输入正确的路径或点击“浏览”。\r\nPathLabel=路径(&P)：\r\nFileNotInDir2=“%2”中找不到文件“%1”。请插入正确的磁盘或选择其他文件夹。\r\nSelectDirectoryLabel=请指定下一张磁盘的位置。\r\n\r\n; *** 安装状态消息\r\nSetupAborted=安装程序未完成安装。%n%n请修正这个问题并重新运行安装程序。\r\nAbortRetryIgnoreSelectAction=选择操作\r\nAbortRetryIgnoreRetry=重试(&T)\r\nAbortRetryIgnoreIgnore=忽略错误并继续(&I)\r\nAbortRetryIgnoreCancel=关闭安装程序\r\n\r\n; *** 安装状态消息\r\nStatusClosingApplications=正在关闭应用程序...\r\nStatusCreateDirs=正在创建目录...\r\nStatusExtractFiles=正在解压缩文件...\r\nStatusCreateIcons=正在创建快捷方式...\r\nStatusCreateIniEntries=正在创建 INI 条目...\r\nStatusCreateRegistryEntries=正在创建注册表条目...\r\nStatusRegisterFiles=正在注册文件...\r\nStatusSavingUninstall=正在保存卸载信息...\r\nStatusRunProgram=正在完成安装...\r\nStatusRestartingApplications=正在重启应用程序...\r\nStatusRollback=正在撤销更改...\r\n\r\n; *** 其他错误\r\nErrorInternal2=内部错误：%1\r\nErrorFunctionFailedNoCode=%1 失败\r\nErrorFunctionFailed=%1 失败；错误代码 %2\r\nErrorFunctionFailedWithMessage=%1 失败；错误代码 %2.%n%3\r\nErrorExecutingProgram=无法执行文件：%n%1\r\n\r\n; *** 注册表错误\r\nErrorRegOpenKey=打开注册表项时出错：%n%1\\%2\r\nErrorRegCreateKey=创建注册表项时出错：%n%1\\%2\r\nErrorRegWriteKey=写入注册表项时出错：%n%1\\%2\r\n\r\n; *** INI 错误\r\nErrorIniEntry=在文件“%1”中创建 INI 条目时出错。\r\n\r\n; *** 文件复制错误\r\nFileAbortRetryIgnoreSkipNotRecommended=跳过此文件(&S) (不推荐)\r\nFileAbortRetryIgnoreIgnoreNotRecommended=忽略错误并继续(&I) (不推荐)\r\nSourceIsCorrupted=源文件已损坏\r\nSourceDoesntExist=源文件“%1”不存在\r\nExistingFileReadOnly2=无法替换现有文件，它是只读的。\r\nExistingFileReadOnlyRetry=移除只读属性并重试(&R)\r\nExistingFileReadOnlyKeepExisting=保留现有文件(&K)\r\nErrorReadingExistingDest=尝试读取现有文件时出错：\r\nFileExistsSelectAction=选择操作\r\nFileExists2=文件已经存在。\r\nFileExistsOverwriteExisting=覆盖已存在的文件(&O)\r\nFileExistsKeepExisting=保留现有的文件(&K)\r\nFileExistsOverwriteOrKeepAll=为所有冲突文件执行此操作(&D)\r\nExistingFileNewerSelectAction=选择操作\r\nExistingFileNewer2=现有的文件比安装程序将要安装的文件还要新。\r\nExistingFileNewerOverwriteExisting=覆盖已存在的文件(&O)\r\nExistingFileNewerKeepExisting=保留现有的文件(&K) (推荐)\r\nExistingFileNewerOverwriteOrKeepAll=为所有冲突文件执行此操作(&D)\r\nErrorChangingAttr=尝试更改下列现有文件的属性时出错：\r\nErrorCreatingTemp=尝试在目标目录创建文件时出错：\r\nErrorReadingSource=尝试读取下列源文件时出错：\r\nErrorCopying=尝试复制下列文件时出错：\r\nErrorReplacingExistingFile=尝试替换现有文件时出错：\r\nErrorRestartReplace=重启并替换失败：\r\nErrorRenamingTemp=尝试重命名下列目标目录中的一个文件时出错：\r\nErrorRegisterServer=无法注册 DLL/OCX：%1\r\nErrorRegSvr32Failed=RegSvr32 失败；退出代码 %1\r\nErrorRegisterTypeLib=无法注册类库：%1\r\n\r\n; *** 卸载显示名字标记\r\n; used for example as 'My Program (32-bit)'\r\nUninstallDisplayNameMark=%1 (%2)\r\n; used for example as 'My Program (32-bit, All users)'\r\nUninstallDisplayNameMarks=%1 (%2, %3)\r\nUninstallDisplayNameMark32Bit=32 位\r\nUninstallDisplayNameMark64Bit=64 位\r\nUninstallDisplayNameMarkAllUsers=所有用户\r\nUninstallDisplayNameMarkCurrentUser=当前用户\r\n\r\n; *** 安装后错误\r\nErrorOpeningReadme=尝试打开自述文件时出错。\r\nErrorRestartingComputer=安装程序无法重启电脑，请手动重启。\r\n\r\n; *** 卸载消息\r\nUninstallNotFound=文件“%1”不存在。无法卸载。\r\nUninstallOpenError=文件“%1”不能被打开。无法卸载。\r\nUninstallUnsupportedVer=此版本的卸载程序无法识别卸载日志文件“%1”的格式。无法卸载\r\nUninstallUnknownEntry=卸载日志中遇到一个未知条目 (%1)\r\nConfirmUninstall=您确认要完全移除 %1 及其所有组件吗？\r\nUninstallOnlyOnWin64=仅允许在 64 位 Windows 中卸载此程序。\r\nOnlyAdminCanUninstall=仅使用管理员权限的用户能完成此卸载。\r\nUninstallStatusLabel=正在从您的电脑中移除 %1，请稍候。\r\nUninstalledAll=已顺利从您的电脑中移除 %1。\r\nUninstalledMost=%1 卸载完成。%n%n有部分内容未能被删除，但您可以手动删除它们。\r\nUninstalledAndNeedsRestart=为完成 %1 的卸载，需要重启您的电脑。%n%n立即重启电脑吗？\r\nUninstallDataCorrupted=文件“%1”已损坏。无法卸载\r\n\r\n; *** 卸载状态消息\r\nConfirmDeleteSharedFileTitle=删除共享的文件吗？\r\nConfirmDeleteSharedFile2=系统表示下列共享的文件已不有其他程序使用。您希望卸载程序删除这些共享的文件吗？%n%n如果删除这些文件，但仍有程序在使用这些文件，则这些程序可能出现异常。如果您不能确定，请选择“否”，在系统中保留这些文件以免引发问题。\r\nSharedFileNameLabel=文件名：\r\nSharedFileLocationLabel=位置：\r\nWizardUninstalling=卸载状态\r\nStatusUninstalling=正在卸载 %1...\r\n\r\n; *** Shutdown block reasons\r\nShutdownBlockReasonInstallingApp=正在安装 %1。\r\nShutdownBlockReasonUninstallingApp=正在卸载 %1。\r\n\r\n; The custom messages below aren't used by Setup itself, but if you make\r\n; use of them in your scripts, you'll want to translate them.\r\n\r\n[CustomMessages]\r\n\r\nNameAndVersion=%1 版本 %2\r\nAdditionalIcons=附加快捷方式：\r\nCreateDesktopIcon=创建桌面快捷方式(&D)\r\nCreateQuickLaunchIcon=创建快速启动栏快捷方式(&Q)\r\nProgramOnTheWeb=%1 网站\r\nUninstallProgram=卸载 %1\r\nLaunchProgram=运行 %1\r\nAssocFileExtension=将 %2 文件扩展名与 %1 建立关联(&A)\r\nAssocingFileExtension=正在将 %2 文件扩展名与 %1 建立关联...\r\nAutoStartProgramGroupDescription=启动：\r\nAutoStartProgram=自动启动 %1\r\nAddonHostProgramNotFound=您选择的文件夹中无法找到 %1。%n%n您要继续吗？"
            , encoding: Encoding.UTF8);

        if (Configuration.StartCommand != null &&
            !Configuration.StartCommand.Equals(AppExecName, StringComparison.InvariantCultureIgnoreCase))
        {
            var path = Path.Combine(BuildAppBin, Configuration.StartCommand + ".bat");
            var script = $"start {InstallExec} %*";
            Operations.WriteFile(path, script);
        }

        if (Configuration.SetupCommandPrompt != null)
        {
            var title = EscapeBat(Configuration.SetupCommandPrompt);
            var cmd = EscapeBat(Configuration.StartCommand ?? Configuration.AppBaseName);
            var path  = Path.Combine(BuildAppBin, PromptBat);

            var echoCopy = Configuration.PublisherCopyright != null ? $"& echo {EscapeBat(Configuration.PublisherCopyright)}" : null;

            var script = $"start cmd /k \"cd /D %userprofile% & title {title} & echo {cmd} {AppVersion} {echoCopy} & set path=%path%;%~dp0\"";
            Operations.WriteFile(path, script);

        }
    }

    private static string? EscapeBat(string? s)
    {
        // \ & | > < ^
        s = s?.Replace("^", "^^");

        s = s?.Replace("\\", "^\\");
        s = s?.Replace("&", "^&");
        s = s?.Replace("|", "^|");
        s = s?.Replace("<", "^<");
        s = s?.Replace(">", "^>");

        s = s?.Replace("%", "");

        return s;
    }

    private string GetInnoFile()
    {
        // We don't actually need install, build sections.
        var sb = new StringBuilder();

        sb.AppendLine($"[Setup]");
        sb.AppendLine($"AppName={Configuration.AppFriendlyName}");
        sb.AppendLine($"AppId={Configuration.AppId}");
        sb.AppendLine($"AppVersion={AppVersion}");
        sb.AppendLine($"AppVerName={Configuration.AppFriendlyName} {AppVersion}");
        sb.AppendLine($"VersionInfoVersion={AppVersion}");
        sb.AppendLine($"OutputDir={OutputDirectory}");
        sb.AppendLine($"OutputBaseFilename={Path.GetFileNameWithoutExtension(OutputName)}");
        sb.AppendLine($"AppPublisher={Configuration.PublisherName}");
        sb.AppendLine($"AppCopyright={Configuration.PublisherCopyright}");
        sb.AppendLine($"AppPublisherURL={Configuration.PublisherLinkUrl}");
        sb.AppendLine($"InfoBeforeFile={Configuration.AppChangeFile}");
        sb.AppendLine($"LicenseFile={Configuration.AppLicenseFile}");
        sb.AppendLine($"SetupIconFile={PrimaryIcon}");
        sb.AppendLine($"AllowNoIcons=yes");
        sb.AppendLine($"MinVersion={Configuration.SetupMinWindowsVersion}");

        sb.AppendLine($"DefaultDirName={{autopf}}\\{Configuration.SetupGroupName ?? Configuration.AppBaseName}");
        sb.AppendLine($"DefaultGroupName={Configuration.SetupGroupName ?? Configuration.AppFriendlyName}");

        if (Architecture == "x64" || Architecture == "arm64")
        {
            // https://jrsoftware.org/ishelp/index.php?topic=setup_architecturesallowed
            sb.AppendLine($"ArchitecturesAllowed={Architecture}");

            // https://jrsoftware.org/ishelp/index.php?topic=setup_architecturesinstallin64bitmode
            sb.AppendLine($"ArchitecturesInstallIn64BitMode={Architecture}");
        }


        sb.AppendLine($"PrivilegesRequired={(Configuration.SetupAdminInstall ? "admin" : "lowest")}");

        if (PrimaryIcon != null)
        {
            sb.AppendLine($"UninstallDisplayIcon={{app}}\\{Path.GetFileName(PrimaryIcon)}");
        }

        if (!string.IsNullOrEmpty(Configuration.SetupSignTool))
        {
            // SetupSignTool = \"C:/Program Files (x86)/Windows Kits/10/bin/10.0.22621.0/x64/signtool.exe" sign /f "{#GetEnv('SigningCertificate')}" /p "{#GetEnv('SigningCertificatePassword')}" /tr http://timestamp.sectigo.com /td sha256 /fd sha256 $f
            sb.AppendLine($"SignTool={Configuration.SetupSignTool}");
        }

        sb.AppendLine();
        sb.AppendLine($"[Files]");
        sb.AppendLine($"Source: \"{BuildAppBin}\\*.exe\"; DestDir: \"{{app}}\"; Flags: ignoreversion recursesubdirs createallsubdirs signonce;");
        sb.AppendLine($"Source: \"{BuildAppBin}\\*.dll\"; DestDir: \"{{app}}\"; Flags: ignoreversion recursesubdirs createallsubdirs signonce;");
        sb.AppendLine($"Source: \"{BuildAppBin}\\*\"; Excludes: \"*.exe,*.dll\"; DestDir: \"{{app}}\"; Flags: ignoreversion recursesubdirs createallsubdirs;");

        if (PrimaryIcon != null)
        {
            sb.AppendLine($"Source: \"{PrimaryIcon}\"; DestDir: \"{{app}}\"; Flags: ignoreversion recursesubdirs createallsubdirs;");
        }

        if (Configuration.SetupCommandPrompt != null)
        {
            // Need this below
            sb.AppendLine($"Source: \"{TerminalIcon}\"; DestDir: \"{{app}}\"; Flags: ignoreversion recursesubdirs createallsubdirs;");
        }

        sb.AppendLine();
        sb.AppendLine("[Tasks]");

        if (!Configuration.DesktopNoDisplay)
        {
            sb.AppendLine($"Name: \"desktopicon\"; Description: \"Create a &Desktop Icon\"; GroupDescription: \"Additional icons:\"; Flags: unchecked");
        }

        sb.AppendLine();
        sb.AppendLine($"[REGISTRY]");
        sb.AppendLine();
        sb.AppendLine($"[Icons]");

        if (!Configuration.DesktopNoDisplay)
        {
            sb.AppendLine($"Name: \"{{group}}\\{Configuration.AppFriendlyName}\"; Filename: \"{{app}}\\{AppExecName}\"");
            sb.AppendLine($"Name: \"{{userdesktop}}\\{Configuration.AppFriendlyName}\"; Filename: \"{{app}}\\{AppExecName}\"; Tasks: desktopicon");
        }

        // Still put CommandPrompt and Home Page link DesktopNoDisplay is true
        if (Configuration.SetupCommandPrompt != null)
        {
            // Give special terminal icon rather meaningless default .bat icon
            var name = Path.GetFileName(TerminalIcon);
            sb.AppendLine($"Name: \"{{group}}\\{Configuration.SetupCommandPrompt}\"; Filename: \"{{app}}\\{PromptBat}\"; IconFilename: \"{{app}}\\{name}\"");
        }

        if (Configuration.PublisherLinkName != null && Configuration.PublisherLinkUrl != null)
        {
            sb.AppendLine($"Name: \"{{group}}\\{Configuration.PublisherLinkName}\"; Filename: \"{Configuration.PublisherLinkUrl}\"");
        }

        sb.AppendLine();
        sb.AppendLine($"[Run]");

        if (!Configuration.DesktopNoDisplay)
        {
            sb.AppendLine($"Filename: \"{{app}}\\{AppExecName}\"; Description: Start Application Now; Flags: postinstall nowait skipifsilent");
        }

        sb.AppendLine();
        sb.AppendLine("[InstallDelete]");
        sb.AppendLine("Type: filesandordirs; Name: \"{app}\\*\";");
        sb.AppendLine("Type: filesandordirs; Name: \"{group}\\*\";");
        sb.AppendLine();
        sb.AppendLine("[UninstallRun]");
        if (!string.IsNullOrEmpty(Configuration.SetupUninstallScript))
        {
            string uninstallScriptPath = $"{{app}}\\{Configuration.SetupUninstallScript}";
            sb.AppendLine($"Filename: \"{uninstallScriptPath}\"; Flags: runhidden waituntilterminated");
        }
        sb.AppendLine();
        sb.AppendLine("[UninstallDelete]");
        sb.AppendLine("Type: dirifempty; Name: \"{app}\"");

        sb.AppendLine("#sub ProcessFoundLanguagesFile"); 
        sb.AppendLine("  #define FileName FindGetFileName(FindHandle)"); 
        sb.AppendLine("  #define Name LowerCase(RemoveFileExt(FileName))"); 
        sb.AppendLine("  #define MessagesFile FindPathName + FileName"); 
        sb.AppendLine("    #pragma message \"Generating [Languages] entry with name \" + Name + \": \" + MessagesFile"); 
        sb.AppendLine("Name: {#Name}; MessagesFile: \"{#MessagesFile}\""); 
        sb.AppendLine("#endsub"); 
        sb.AppendLine("#define FindPathName");
        sb.AppendLine("#define FindHandle");
        sb.AppendLine("#define FindResult");
        sb.AppendLine("#sub DoFindFiles");
        sb.AppendLine("  #for {FindHandle = FindResult = FindFirst(FindPathName + \"*.isl\", 0); FindResult; FindResult = FindNext(FindHandle)} ProcessFoundLanguagesFile"); 
        sb.AppendLine("  #if FindHandle"); 
        sb.AppendLine("    #expr FindClose(FindHandle)"); 
        sb.AppendLine("  #endif"); 
        sb.AppendLine("#endsub"); 
        sb.AppendLine("#define FindFiles(str PathName) \\"); 
        sb.AppendLine("  FindPathName = PathName, \\"); 
        sb.AppendLine("  DoFindFiles"); 
        sb.AppendLine("[Languages]"); 
        sb.AppendLine("Name: english; MessagesFile: \"compiler:Default.isl\""); 
        sb.AppendLine("#expr FindFiles(\"compiler:Languages\\\")");
        sb.AppendLine($"#expr FindFiles(\"{Root}\\\")");

        return sb.ToString().TrimEnd();
    }

}

