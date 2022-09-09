using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldOtherPeopleChatItem : MonoBehaviour
{
    public Text playerNameText;
    public Image headIconImg;
    public Image dialogBgImg;
    public Text msgTxt;

    public TextGenerator textGenerator;
    TextGenerationSettings settings;
    object m_Info;

    //气泡的宽度
    float width;
    bool isShow = false;
    // Start is called before the first frame update
    void Awake()
    {
        playerNameText = this.gameObject.transform.Find("PlayerName_Txt").GetComponent<Text>();
        headIconImg = this.gameObject.transform.Find("HeadIcon_Img").GetComponent<Image>();
        dialogBgImg = this.gameObject.transform.Find("DialogBg_Img").GetComponent<Image>();
        msgTxt = this.gameObject.transform.Find("DialogBg_Img/Msg_Txt").GetComponent<Text>();
    }
    public void Init(string str)
    {
        gameObject.SetActive(true);
        msgTxt.text = str;
        width = dialogBgImg.GetComponent<RectTransform>().rect.width;
        textGenerator = msgTxt.cachedTextGeneratorForLayout;
        OnText();
    }
    void OnText()
    {
        settings = msgTxt.GetGenerationSettings(Vector2.zero);
        Canvas.ForceUpdateCanvases();

        float textHeight = msgTxt.GetComponent<RectTransform>().rect.height;
        float textWidth = textGenerator.GetPreferredWidth(msgTxt.text, settings) / msgTxt.pixelsPerUnit;

        if (textWidth <= width)
        {
            dialogBgImg.GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth + 75, textHeight + 30);
            dialogBgImg.GetComponent<RectTransform>().anchoredPosition = new Vector2((textWidth / 2) + 106 + 75 / 2, -60);
        }
        else
        {
            dialogBgImg.GetComponent<RectTransform>().sizeDelta = new Vector2(width, textHeight + 30);
            dialogBgImg.GetComponent<RectTransform>().anchoredPosition = new Vector2((width / 2) + 106, -60);
        }
        isShow = true;
    }
    public void LateUpdate()
    {
        if (isShow)
        {
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(gameObject.GetComponent<RectTransform>().rect.width, msgTxt.GetComponent<RectTransform>().rect.height + 100);
            isShow = false;
        }
    }
}

