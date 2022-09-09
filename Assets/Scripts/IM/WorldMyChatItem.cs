using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMyChatItem : MonoBehaviour
{
    public Text playerNameText;
    public Image headIconImg;
    public Image dialogBgImg;
    public Text msgTxt;
    private Dictionary<int, Sprite> avaters;


    public TextGenerator textGenerator;
    TextGenerationSettings settings;

    float width;//气泡的宽度
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
        //获取显示文本的长和宽
        float textHeight = msgTxt.GetComponent<RectTransform>().rect.height;
        float textWidth = textGenerator.GetPreferredWidth(msgTxt.text, settings) / msgTxt.pixelsPerUnit;
        //修改聊天背景框的大小
        if (textWidth <= width)
        {
            dialogBgImg.GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth + 70, textHeight + 30);
            dialogBgImg.GetComponent<RectTransform>().anchoredPosition = new Vector2((textWidth / 2 * -1) - 210, -60);
        }
        else
        {
            dialogBgImg.GetComponent<RectTransform>().sizeDelta = new Vector2(width + 70, textHeight + 30);
            dialogBgImg.GetComponent<RectTransform>().anchoredPosition = new Vector2((width / 2 * -1) - 210, -60);
        }
        isShow = true;
    }
    void LateUpdate()
    {
        if (isShow)
        {
            //修改item的大小，排列使用
            //放在LateUpdate，延迟渲染
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(gameObject.GetComponent<RectTransform>().rect.width, msgTxt.GetComponent<RectTransform>().rect.height + 100);
            isShow = false;
        }
    }
}

