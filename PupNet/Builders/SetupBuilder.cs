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
        Operations.WriteFile(zhcnLanguagePath, "; *** Inno Setup version 6.1.0+ Chinese Simplified messages ***\r\n;\r\n; To download user-contributed translations of this file, go to:\r\n;   https://jrsoftware.org/files/istrans/\r\n;\r\n; Note: When translating this text, do not add periods (.) to the end of\r\n; messages that didn't have them already, because on those messages Inno\r\n; Setup adds the periods automatically (appending a period would result in\r\n; two periods being displayed).\r\n;\r\n; Maintained by Zhenghan Yang\r\n; Email: 847320916@QQ.com\r\n; Translation based on network resource\r\n; The latest Translation is on https://github.com/kira-96/Inno-Setup-Chinese-Simplified-Translation\r\n;\r\n\r\n[LangOptions]\r\n; The following three entries are very important. Be sure to read and \r\n; understand the '[LangOptions] section' topic in the help file.\r\nLanguageName=��������\r\n; If Language Name display incorrect, uncomment next line\r\n; LanguageName=<7B80><4F53><4E2D><6587>\r\n; About LanguageID, to reference link:\r\n; https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c\r\nLanguageID=$0804\r\n; About CodePage, to reference link:\r\n; https://docs.microsoft.com/en-us/windows/win32/intl/code-page-identifiers\r\nLanguageCodePage=936\r\n; If the language you are translating to requires special font faces or\r\n; sizes, uncomment any of the following entries and change them accordingly.\r\n;DialogFontName=\r\n;DialogFontSize=8\r\n;WelcomeFontName=Verdana\r\n;WelcomeFontSize=12\r\n;TitleFontName=Arial\r\n;TitleFontSize=29\r\n;CopyrightFontName=Arial\r\n;CopyrightFontSize=8\r\n\r\n[Messages]\r\n\r\n; *** Ӧ�ó������\r\nSetupAppTitle=��װ\r\nSetupWindowTitle=��װ - %1\r\nUninstallAppTitle=ж��\r\nUninstallAppFullTitle=%1 ж��\r\n\r\n; *** Misc. common\r\nInformationTitle=��Ϣ\r\nConfirmTitle=ȷ��\r\nErrorTitle=����\r\n\r\n; *** SetupLdr messages\r\nSetupLdrStartupMessage=���ڽ���װ %1������Ҫ������\r\nLdrCannotCreateTemp=�޷�������ʱ�ļ�����װ��������ֹ\r\nLdrCannotExecTemp=�޷�ִ����ʱĿ¼�е��ļ�����װ��������ֹ\r\nHelpTextNote=\r\n\r\n; *** ����������Ϣ\r\nLastErrorMessage=%1��%n%n���� %2: %3\r\nSetupFileMissing=��װĿ¼��ȱ���ļ� %1�����������������߻�ȡ������¸�����\r\nSetupFileCorrupt=��װ�ļ����𻵡����ȡ������¸�����\r\nSetupFileCorruptOrWrongVer=��װ�ļ����𻵣������������װ����İ汾�����ݡ����������������ȡ�µĳ��򸱱���\r\nInvalidParameter=��Ч�������в�����%n%n%1\r\nSetupAlreadyRunning=��װ�����������С�\r\nWindowsVersionNotSupported=�˳���֧�ֵ�ǰ��������е� Windows �汾��\r\nWindowsServicePackRequired=�˳�����Ҫ %1 ����� %2 ����߰汾��\r\nNotOnThisPlatform=�˳������� %1 �����С�\r\nOnlyOnThisPlatform=�˳���ֻ���� %1 �����С�\r\nOnlyOnTheseArchitectures=�˳���ֻ����Ϊ���д������ṹ��Ƶ� Windows �汾�а�װ��%n%n%1\r\nWinVersionTooLowError=�˳�����Ҫ %1 �汾 %2 ����ߡ�\r\nWinVersionTooHighError=�˳����ܰ�װ�� %1 �汾 %2 ����ߡ�\r\nAdminPrivilegesRequired=�ڰ�װ�˳���ʱ�������Թ���Ա��ݵ�¼��\r\nPowerUserPrivilegesRequired=�ڰ�װ�˳���ʱ�������Թ���Ա��ݻ���Ȩ�޵��û�����ݵ�¼��\r\nSetupAppRunningError=��װ������ %1 ��ǰ�������С�%n%n���ȹر��������еĳ���Ȼ������ȷ����������������ȡ�����˳���\r\nUninstallAppRunningError=ж�س����� %1 ��ǰ�������С�%n%n���ȹر��������еĳ���Ȼ������ȷ����������������ȡ�����˳���\r\n\r\n; *** ��������\r\nPrivilegesRequiredOverrideTitle=ѡ��װ����ģʽ\r\nPrivilegesRequiredOverrideInstruction=ѡ��װģʽ\r\nPrivilegesRequiredOverrideText1=%1 ����Ϊ�����û���װ(��Ҫ����ԱȨ��)�����Ϊ����װ��\r\nPrivilegesRequiredOverrideText2=%1 ֻ��Ϊ����װ����Ϊ�����û���װ(��Ҫ����ԱȨ��)��\r\nPrivilegesRequiredOverrideAllUsers=Ϊ�����û���װ(&A)\r\nPrivilegesRequiredOverrideAllUsersRecommended=Ϊ�����û���װ(&A) (����ѡ��)\r\nPrivilegesRequiredOverrideCurrentUser=ֻΪ�Ұ�װ(&M)\r\nPrivilegesRequiredOverrideCurrentUserRecommended=ֻΪ�Ұ�װ(&M) (����ѡ��)\r\n\r\n; *** ��������\r\nErrorCreatingDir=��װ�����޷�����Ŀ¼��%1����\r\nErrorTooManyFilesInDir=�޷���Ŀ¼��%1���д����ļ�����Ϊ�������̫���ļ�\r\n\r\n; *** ��װ���򹫹���Ϣ\r\nExitSetupTitle=�˳���װ����\r\nExitSetupMessage=��װ������δ��ɡ���������˳��������ᰲװ�ó���%n%n��֮������ٴ����а�װ������ɰ�װ��%n%n�����˳���װ������\r\nAboutSetupMenuItem=���ڰ�װ����(&A)...\r\nAboutSetupTitle=���ڰ�װ����\r\nAboutSetupMessage=%1 �汾 %2%n%3%n%n%1 ��ҳ��%n%4\r\nAboutSetupNote=\r\nTranslatorNote=�������ķ�����Kira(847320916@qq.com)ά������Ŀ��ַ��https://github.com/kira-96/Inno-Setup-Chinese-Simplified-Translation\r\n\r\n; *** ��ť\r\nButtonBack=< ��һ��(&B)\r\nButtonNext=��һ��(&N) >\r\nButtonInstall=��װ(&I)\r\nButtonOK=ȷ��\r\nButtonCancel=ȡ��\r\nButtonYes=��(&Y)\r\nButtonYesToAll=ȫ��(&A)\r\nButtonNo=��(&N)\r\nButtonNoToAll=ȫ��(&O)\r\nButtonFinish=���(&F)\r\nButtonBrowse=���(&B)...\r\nButtonWizardBrowse=���(&R)...\r\nButtonNewFolder=�½��ļ���(&M)\r\n\r\n; *** ��ѡ�����ԡ��Ի�����Ϣ\r\nSelectLanguageTitle=ѡ��װ����\r\nSelectLanguageLabel=ѡ��װʱʹ�õ����ԡ�\r\n\r\n; *** ����������\r\nClickNext=�������һ����������������ȡ�����˳���װ����\r\nBeveledLabel=\r\nBrowseDialogTitle=����ļ���\r\nBrowseDialogLabel=��������б���ѡ��һ���ļ��У�Ȼ������ȷ������\r\nNewFolderName=�½��ļ���\r\n\r\n; *** ����ӭ����ҳ\r\nWelcomeLabel1=��ӭʹ�� [name] ��װ��\r\nWelcomeLabel2=���ڽ���װ [name/ver] �����ĵ����С�%n%n�������ڼ�����װǰ�ر���������Ӧ�ó���\r\n\r\n; *** �����롱��ҳ\r\nWizardPassword=����\r\nPasswordLabel1=�����װ���������뱣����\r\nPasswordLabel3=���������룬Ȼ��������һ�����������������ִ�Сд��\r\nPasswordEditLabel=����(&P)��\r\nIncorrectPassword=����������벻��ȷ�����������롣\r\n\r\n; *** �����Э�顱��ҳ\r\nWizardLicense=���Э��\r\nLicenseLabel=���ڼ�����װǰ�Ķ�������Ҫ��Ϣ��\r\nLicenseLabel3=����ϸ�Ķ��������Э�顣�ڼ�����װǰ������ͬ����ЩЭ�����\r\nLicenseAccepted=��ͬ���Э��(&A)\r\nLicenseNotAccepted=�Ҳ�ͬ���Э��(&D)\r\n\r\n; *** ����Ϣ����ҳ\r\nWizardInfoBefore=��Ϣ\r\nInfoBeforeLabel=���ڼ�����װǰ�Ķ�������Ҫ��Ϣ��\r\nInfoBeforeClickLabel=׼���ü�����װ�󣬵������һ������\r\nWizardInfoAfter=��Ϣ\r\nInfoAfterLabel=���ڼ�����װǰ�Ķ�������Ҫ��Ϣ��\r\nInfoAfterClickLabel=׼���ü�����װ�󣬵������һ������\r\n\r\n; *** ���û���Ϣ����ҳ\r\nWizardUserInfo=�û���Ϣ\r\nUserInfoDesc=������������Ϣ��\r\nUserInfoName=�û���(&U)��\r\nUserInfoOrg=��֯(&O)��\r\nUserInfoSerial=���к�(&S)��\r\nUserInfoNameRequired=�����������û�����\r\n\r\n; *** ��ѡ��Ŀ��Ŀ¼����ҳ\r\nWizardSelectDir=ѡ��Ŀ��λ��\r\nSelectDirDesc=���뽫 [name] ��װ�����\r\nSelectDirLabel3=��װ���򽫰�װ [name] ��������ļ����С�\r\nSelectDirBrowseLabel=�������һ�����������������ѡ�������ļ��У�������������\r\nDiskSpaceGBLabel=������Ҫ�� [gb] GB �Ŀ��ô��̿ռ䡣\r\nDiskSpaceMBLabel=������Ҫ�� [mb] MB �Ŀ��ô��̿ռ䡣\r\nCannotInstallToNetworkDrive=��װ�����޷���װ��һ��������������\r\nCannotInstallToUNCPath=��װ�����޷���װ��һ�� UNC ·����\r\nInvalidPath=����������һ������������������·�������磺%n%nC:\\APP%n%n��UNC·����%n%n\\\\server\\share\r\nInvalidDrive=��ѡ������������ UNC �������ڻ��ܷ��ʡ���ѡ������λ�á�\r\nDiskSpaceWarningTitle=���̿ռ䲻��\r\nDiskSpaceWarning=��װ����������Ҫ %1 KB �Ŀ��ÿռ���ܰ�װ����ѡ��������ֻ�� %2 KB �Ŀ��ÿռ䡣%n%n��һ��Ҫ������\r\nDirNameTooLong=�ļ������ƻ�·��̫����\r\nInvalidDirName=�ļ���������Ч��\r\nBadDirName32=�ļ������Ʋ��ܰ��������κ��ַ���%n%n%1\r\nDirExistsTitle=�ļ����Ѵ���\r\nDirExists=�ļ��У�%n%n%1%n%n�Ѿ����ڡ���һ��Ҫ��װ������ļ�������\r\nDirDoesntExistTitle=�ļ��в�����\r\nDirDoesntExist=�ļ��У�%n%n%1%n%n�����ڡ�����Ҫ�������ļ�����\r\n\r\n; *** ��ѡ���������ҳ\r\nWizardSelectComponents=ѡ�����\r\nSelectComponentsDesc=���밲װ��Щ���������\r\nSelectComponentsLabel2=ѡ�����밲װ�������ȡ�������밲װ�������Ȼ��������һ����������\r\nFullInstallation=��ȫ��װ\r\n; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)\r\nCompactInstallation=��లװ\r\nCustomInstallation=�Զ��尲װ\r\nNoUninstallWarningTitle=����Ѵ���\r\nNoUninstallWarning=��װ�����⵽��������Ѱ�װ�����ĵ����У�%n%n%1%n%nȡ��ѡ����Щ�������ж�����ǡ�%n%nȷ��Ҫ������\r\nComponentSize1=%1 KB\r\nComponentSize2=%1 MB\r\nComponentsDiskSpaceGBLabel=��ǰѡ��������Ҫ���� [gb] GB �Ĵ��̿ռ䡣\r\nComponentsDiskSpaceMBLabel=��ǰѡ��������Ҫ���� [mb] MB �Ĵ��̿ռ䡣\r\n\r\n; *** ��ѡ�񸽼�������ҳ\r\nWizardSelectTasks=ѡ�񸽼�����\r\nSelectTasksDesc=����Ҫ��װ����ִ����Щ��������\r\nSelectTasksLabel2=ѡ������Ҫ��װ�����ڰ�װ [name] ʱִ�еĸ�������Ȼ��������һ������\r\n\r\n; *** ��ѡ��ʼ�˵��ļ��С���ҳ\r\nWizardSelectProgramGroup=ѡ��ʼ�˵��ļ���\r\nSelectStartMenuFolderDesc=��װ����Ӧ����������ó���Ŀ�ݷ�ʽ��\r\nSelectStartMenuFolderLabel3=��װ���������С���ʼ���˵��ļ����д�������Ŀ�ݷ�ʽ��\r\nSelectStartMenuFolderBrowseLabel=�������һ�����������������ѡ�������ļ��У�������������\r\nMustEnterGroupName=����������һ���ļ�������\r\nGroupNameTooLong=�ļ�������·��̫����\r\nInvalidGroupName=��Ч���ļ������֡�\r\nBadGroupName=�ļ��������ܰ��������κ��ַ���%n%n%1\r\nNoProgramGroupCheck2=��������ʼ�˵��ļ���(&D)\r\n\r\n; *** ��׼����װ����ҳ\r\nWizardReady=׼����װ\r\nReadyLabel1=��װ����׼�����������ڿ��Կ�ʼ��װ [name] �����ĵ��ԡ�\r\nReadyLabel2a=�������װ�������˰�װ��������������¿��ǻ��޸��κ����ã��������һ������\r\nReadyLabel2b=�������װ�������˰�װ����\r\nReadyMemoUserInfo=�û���Ϣ��\r\nReadyMemoDir=Ŀ��λ�ã�\r\nReadyMemoType=��װ���ͣ�\r\nReadyMemoComponents=��ѡ�������\r\nReadyMemoGroup=��ʼ�˵��ļ��У�\r\nReadyMemoTasks=��������\r\n\r\n; *** TDownloadWizardPage wizard page and DownloadTemporaryFile\r\nDownloadingLabel=�������ظ����ļ�...\r\nButtonStopDownload=ֹͣ����(&S)\r\nStopDownload=��ȷ��Ҫֹͣ������\r\nErrorDownloadAborted=��������ֹ\r\nErrorDownloadFailed=����ʧ�ܣ�%1 %2\r\nErrorDownloadSizeFailed=��ȡ���ش�Сʧ�ܣ�%1 %2\r\nErrorFileHash1=У���ļ���ϣʧ�ܣ�%1\r\nErrorFileHash2=��Ч���ļ���ϣ��Ԥ�� %1��ʵ�� %2\r\nErrorProgress=��Ч�Ľ��ȣ�%1 / %2\r\nErrorFileSize=�ļ���С����Ԥ�� %1��ʵ�� %2\r\n\r\n; *** ������׼����װ����ҳ\r\nWizardPreparing=����׼����װ\r\nPreparingDesc=��װ��������׼����װ [name] �����ĵ��ԡ�\r\nPreviousInstallNotCompleted=��ǰ�ĳ���װ��ж��δ��ɣ�����Ҫ�������ĵ�������ɡ�%n%n���������Ժ��ٴ����а�װ��������� [name] �İ�װ��\r\nCannotContinue=��װ�����ܼ�����������ȡ�����˳���\r\nApplicationsFound=����Ӧ�ó�������ʹ�ý��ɰ�װ������µ��ļ�������������װ�����Զ��ر���ЩӦ�ó���\r\nApplicationsFound2=����Ӧ�ó�������ʹ�ý��ɰ�װ������µ��ļ�������������װ�����Զ��ر���ЩӦ�ó��򡣰�װ��ɺ󣬰�װ���򽫳�������������ЩӦ�ó���\r\nCloseApplications=�Զ��ر�Ӧ�ó���(&A)\r\nDontCloseApplications=��Ҫ�ر�Ӧ�ó���(&D)\r\nErrorCloseApplications=��װ�����޷��Զ��ر�����Ӧ�ó��򡣽������ڼ���֮ǰ���ر�������ʹ����Ҫ�ɰ�װ������µ��ļ���Ӧ�ó���\r\nPrepareToInstallNeedsRestart=��װ��������������ļ��������������������ٴ����а�װ��������� [name] �İ�װ��%n%n�Ƿ���������������\r\n\r\n; *** �����ڰ�װ����ҳ\r\nWizardInstalling=���ڰ�װ\r\nInstallingLabel=��װ�������ڰ�װ [name] �����ĵ��ԣ����Ժ�\r\n\r\n; *** ����װ��ɡ���ҳ\r\nFinishedHeadingLabel=[name] ��װ���\r\nFinishedLabelNoIcons=��װ�����������ĵ����а�װ�� [name]��\r\nFinishedLabel=��װ�����������ĵ����а�װ�� [name]��������ͨ���Ѱ�װ�Ŀ�ݷ�ʽ���д�Ӧ�ó���\r\nClickFinish=�������ɡ��˳���װ����\r\nFinishedRestartLabel=Ϊ��� [name] �İ�װ����װ������������������ĵ��ԡ�Ҫ����������\r\nFinishedRestartMessage=Ϊ��� [name] �İ�װ����װ������������������ĵ��ԡ�%n%nҪ����������\r\nShowReadmeCheck=�ǣ�������������ļ�\r\nYesRadio=�ǣ�������������(&Y)\r\nNoRadio=���Ժ���������(&N)\r\n; used for example as 'Run MyProg.exe'\r\nRunEntryExec=���� %1\r\n; used for example as 'View Readme.txt'\r\nRunEntryShellExec=���� %1\r\n\r\n; *** ����װ������Ҫ��һ�Ŵ��̡���ʾ\r\nChangeDiskTitle=��װ������Ҫ��һ�Ŵ���\r\nSelectDiskLabel2=�������� %1 �������ȷ������%n%n�����������е��ļ������������ļ���֮����ļ������ҵ�����������ȷ��·���������������\r\nPathLabel=·��(&P)��\r\nFileNotInDir2=��%2�����Ҳ����ļ���%1�����������ȷ�Ĵ��̻�ѡ�������ļ��С�\r\nSelectDirectoryLabel=��ָ����һ�Ŵ��̵�λ�á�\r\n\r\n; *** ��װ״̬��Ϣ\r\nSetupAborted=��װ����δ��ɰ�װ��%n%n������������Ⲣ�������а�װ����\r\nAbortRetryIgnoreSelectAction=ѡ�����\r\nAbortRetryIgnoreRetry=����(&T)\r\nAbortRetryIgnoreIgnore=���Դ��󲢼���(&I)\r\nAbortRetryIgnoreCancel=�رհ�װ����\r\n\r\n; *** ��װ״̬��Ϣ\r\nStatusClosingApplications=���ڹر�Ӧ�ó���...\r\nStatusCreateDirs=���ڴ���Ŀ¼...\r\nStatusExtractFiles=���ڽ�ѹ���ļ�...\r\nStatusCreateIcons=���ڴ�����ݷ�ʽ...\r\nStatusCreateIniEntries=���ڴ��� INI ��Ŀ...\r\nStatusCreateRegistryEntries=���ڴ���ע�����Ŀ...\r\nStatusRegisterFiles=����ע���ļ�...\r\nStatusSavingUninstall=���ڱ���ж����Ϣ...\r\nStatusRunProgram=������ɰ�װ...\r\nStatusRestartingApplications=��������Ӧ�ó���...\r\nStatusRollback=���ڳ�������...\r\n\r\n; *** ��������\r\nErrorInternal2=�ڲ�����%1\r\nErrorFunctionFailedNoCode=%1 ʧ��\r\nErrorFunctionFailed=%1 ʧ�ܣ�������� %2\r\nErrorFunctionFailedWithMessage=%1 ʧ�ܣ�������� %2.%n%3\r\nErrorExecutingProgram=�޷�ִ���ļ���%n%1\r\n\r\n; *** ע������\r\nErrorRegOpenKey=��ע�����ʱ����%n%1\\%2\r\nErrorRegCreateKey=����ע�����ʱ����%n%1\\%2\r\nErrorRegWriteKey=д��ע�����ʱ����%n%1\\%2\r\n\r\n; *** INI ����\r\nErrorIniEntry=���ļ���%1���д��� INI ��Ŀʱ����\r\n\r\n; *** �ļ����ƴ���\r\nFileAbortRetryIgnoreSkipNotRecommended=�������ļ�(&S) (���Ƽ�)\r\nFileAbortRetryIgnoreIgnoreNotRecommended=���Դ��󲢼���(&I) (���Ƽ�)\r\nSourceIsCorrupted=Դ�ļ�����\r\nSourceDoesntExist=Դ�ļ���%1��������\r\nExistingFileReadOnly2=�޷��滻�����ļ�������ֻ���ġ�\r\nExistingFileReadOnlyRetry=�Ƴ�ֻ�����Բ�����(&R)\r\nExistingFileReadOnlyKeepExisting=���������ļ�(&K)\r\nErrorReadingExistingDest=���Զ�ȡ�����ļ�ʱ����\r\nFileExistsSelectAction=ѡ�����\r\nFileExists2=�ļ��Ѿ����ڡ�\r\nFileExistsOverwriteExisting=�����Ѵ��ڵ��ļ�(&O)\r\nFileExistsKeepExisting=�������е��ļ�(&K)\r\nFileExistsOverwriteOrKeepAll=Ϊ���г�ͻ�ļ�ִ�д˲���(&D)\r\nExistingFileNewerSelectAction=ѡ�����\r\nExistingFileNewer2=���е��ļ��Ȱ�װ����Ҫ��װ���ļ���Ҫ�¡�\r\nExistingFileNewerOverwriteExisting=�����Ѵ��ڵ��ļ�(&O)\r\nExistingFileNewerKeepExisting=�������е��ļ�(&K) (�Ƽ�)\r\nExistingFileNewerOverwriteOrKeepAll=Ϊ���г�ͻ�ļ�ִ�д˲���(&D)\r\nErrorChangingAttr=���Ը������������ļ�������ʱ����\r\nErrorCreatingTemp=������Ŀ��Ŀ¼�����ļ�ʱ����\r\nErrorReadingSource=���Զ�ȡ����Դ�ļ�ʱ����\r\nErrorCopying=���Ը��������ļ�ʱ����\r\nErrorReplacingExistingFile=�����滻�����ļ�ʱ����\r\nErrorRestartReplace=�������滻ʧ�ܣ�\r\nErrorRenamingTemp=��������������Ŀ��Ŀ¼�е�һ���ļ�ʱ����\r\nErrorRegisterServer=�޷�ע�� DLL/OCX��%1\r\nErrorRegSvr32Failed=RegSvr32 ʧ�ܣ��˳����� %1\r\nErrorRegisterTypeLib=�޷�ע����⣺%1\r\n\r\n; *** ж����ʾ���ֱ��\r\n; used for example as 'My Program (32-bit)'\r\nUninstallDisplayNameMark=%1 (%2)\r\n; used for example as 'My Program (32-bit, All users)'\r\nUninstallDisplayNameMarks=%1 (%2, %3)\r\nUninstallDisplayNameMark32Bit=32 λ\r\nUninstallDisplayNameMark64Bit=64 λ\r\nUninstallDisplayNameMarkAllUsers=�����û�\r\nUninstallDisplayNameMarkCurrentUser=��ǰ�û�\r\n\r\n; *** ��װ�����\r\nErrorOpeningReadme=���Դ������ļ�ʱ����\r\nErrorRestartingComputer=��װ�����޷��������ԣ����ֶ�������\r\n\r\n; *** ж����Ϣ\r\nUninstallNotFound=�ļ���%1�������ڡ��޷�ж�ء�\r\nUninstallOpenError=�ļ���%1�����ܱ��򿪡��޷�ж�ء�\r\nUninstallUnsupportedVer=�˰汾��ж�س����޷�ʶ��ж����־�ļ���%1���ĸ�ʽ���޷�ж��\r\nUninstallUnknownEntry=ж����־������һ��δ֪��Ŀ (%1)\r\nConfirmUninstall=��ȷ��Ҫ��ȫ�Ƴ� %1 �������������\r\nUninstallOnlyOnWin64=�������� 64 λ Windows ��ж�ش˳���\r\nOnlyAdminCanUninstall=��ʹ�ù���ԱȨ�޵��û�����ɴ�ж�ء�\r\nUninstallStatusLabel=���ڴ����ĵ������Ƴ� %1�����Ժ�\r\nUninstalledAll=��˳�������ĵ������Ƴ� %1��\r\nUninstalledMost=%1 ж����ɡ�%n%n�в�������δ�ܱ�ɾ�������������ֶ�ɾ�����ǡ�\r\nUninstalledAndNeedsRestart=Ϊ��� %1 ��ж�أ���Ҫ�������ĵ��ԡ�%n%n��������������\r\nUninstallDataCorrupted=�ļ���%1�����𻵡��޷�ж��\r\n\r\n; *** ж��״̬��Ϣ\r\nConfirmDeleteSharedFileTitle=ɾ��������ļ���\r\nConfirmDeleteSharedFile2=ϵͳ��ʾ���й�����ļ��Ѳ�����������ʹ�á���ϣ��ж�س���ɾ����Щ������ļ���%n%n���ɾ����Щ�ļ��������г�����ʹ����Щ�ļ�������Щ������ܳ����쳣�����������ȷ������ѡ�񡰷񡱣���ϵͳ�б�����Щ�ļ������������⡣\r\nSharedFileNameLabel=�ļ�����\r\nSharedFileLocationLabel=λ�ã�\r\nWizardUninstalling=ж��״̬\r\nStatusUninstalling=����ж�� %1...\r\n\r\n; *** Shutdown block reasons\r\nShutdownBlockReasonInstallingApp=���ڰ�װ %1��\r\nShutdownBlockReasonUninstallingApp=����ж�� %1��\r\n\r\n; The custom messages below aren't used by Setup itself, but if you make\r\n; use of them in your scripts, you'll want to translate them.\r\n\r\n[CustomMessages]\r\n\r\nNameAndVersion=%1 �汾 %2\r\nAdditionalIcons=���ӿ�ݷ�ʽ��\r\nCreateDesktopIcon=���������ݷ�ʽ(&D)\r\nCreateQuickLaunchIcon=����������������ݷ�ʽ(&Q)\r\nProgramOnTheWeb=%1 ��վ\r\nUninstallProgram=ж�� %1\r\nLaunchProgram=���� %1\r\nAssocFileExtension=�� %2 �ļ���չ���� %1 ��������(&A)\r\nAssocingFileExtension=���ڽ� %2 �ļ���չ���� %1 ��������...\r\nAutoStartProgramGroupDescription=������\r\nAutoStartProgram=�Զ����� %1\r\nAddonHostProgramNotFound=��ѡ����ļ������޷��ҵ� %1��%n%n��Ҫ������"
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

