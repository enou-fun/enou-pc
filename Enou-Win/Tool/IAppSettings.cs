using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config.Net;

namespace Enou
{
    public interface IAppSettings
    {
        [Option(Alias ="Enou.AccountToken", DefaultValue ="")]
        string EnouAccountToken
        {
            get;
            set;
        }

        [Option(Alias = "Enou.Account", DefaultValue = "")]
        string EnouAccount
        {
            get;
            set;
        }

        [Option(Alias = "Enou.WordApi", DefaultValue = "")]
        string EnouServerWordApi
        {
            get;
            set;
        }

        [Option(Alias = "Enou.LoginApi", DefaultValue = "")]
        string EnouServerLoginApi
        {
            get;
            set;
        }

        [Option(Alias = "Enou.TokenCheckApi", DefaultValue = "")]
        string EnouServerTokenLoginCheckApi
        {
            get;
            set;
        }

        [Option(Alias = "Software.OnCloseMainWindow", DefaultValue = "Exit")]
        string OnClickCloseButton
        {
            get;
            set;
        }

        [Option(Alias = "Globalization.Language", DefaultValue = "zh-CN")]
        string AppLanguage
        {
            get;
            set;
        }

        [Option(Alias = "JBeijing.JBJCTDllPath", DefaultValue = "")]
        string JBJCTDllPath
        {
            get;
            set;
        }

        [Option(Alias = "KingsoftFastAIT.KingsoftFastAITPath", DefaultValue = "")]
        string KingsoftFastAITPath
        {
            get;
            set;
        }

        [Option(Alias = "Dreye.DreyePath", DefaultValue = "")]
        string DreyePath
        {
            get;
            set;
        }

        [Option(Alias = "TencentOldTranslator.SecretId", DefaultValue = "")]
        string TXOSecretId
        {
            get;
            set;
        }

        [Option(Alias = "TencentOldTranslator.SecretKey", DefaultValue = "")]
        string TXOSecretKey
        {
            get;
            set;
        }

        [Option(Alias = "BaiduTranslator.appID", DefaultValue = "")]
        string BDappID
        {
            get;
            set;
        }

        [Option(Alias = "BaiduTranslator.secretKey", DefaultValue = "")]
        string BDsecretKey
        {
            get;
            set;
        }

        [Option(Alias = "TencentTranslator.appID", DefaultValue = "")]
        string TXappID
        {
            get;
            set;
        }

        [Option(Alias = "TencentTranslator.appKey", DefaultValue = "")]
        string TXappKey
        {
            get;
            set;
        }

        [Option(Alias = "CaiyunTranslator.caiyunToken", DefaultValue = "")]
        string CaiyunToken
        {
            get;
            set;
        }

        [Option(Alias = "XiaoniuTranslator.xiaoniuApiKey", DefaultValue = "")]
        string xiaoniuApiKey
        {
            get;
            set;
        }

        [Option(Alias = "Translate_All.EachRowTrans", DefaultValue = "True")]
        string EachRowTrans
        {
            get;
            set;
        }

        [Option(Alias = "Translate_All.FirstTranslator", DefaultValue = "NoTranslate")]
        string FirstTranslator
        {
            get;
            set;
        }

        [Option(Alias = "Translate_All.SecondTranslator", DefaultValue = "NoTranslate")]
        string SecondTranslator
        {
            get;
            set;
        }

        [Option(Alias = "Translate_All.TransLimitNums", DefaultValue = 100)]
        int TransLimitNums
        {
            get;
            set;
        }

        [Option(Alias = "OCR_All.OCRsource", DefaultValue = "BaiduOCR")]
        string OCRsource
        {
            get;
            set;
        }

        [Option(Alias = "OCR_All.GlobalOCRHotkey", DefaultValue = "Ctrl + Alt + Q")]
        string GlobalOCRHotkey
        {
            get;
            set;
        }

        [Option(Alias = "OCR_All.GlobalOCRLang", DefaultValue = "eng")]
        string GlobalOCRLang
        {
            get;
            set;
        }

        [Option(Alias = "BaiduOCR.APIKEY", DefaultValue = "")]
        string BDOCR_APIKEY
        {
            get;
            set;
        }

        [Option(Alias = "BaiduOCR.SecretKey", DefaultValue = "")]
        string BDOCR_SecretKey
        {
            get;
            set;
        }

        [Option(Alias = "LE.LEPath", DefaultValue = "")]
        string LEPath
        {
            get;
            set;
        }


        #region 界面设置
        #region 前景色设置
        [Option(Alias = "Appearance.Foreground", DefaultValue = "#ffcccc")]
        string ForegroundHex
        {
            get;
            set;
        }
        #endregion
        #endregion

    }

}