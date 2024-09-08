using Assets.Scripts.TypewriterEffects;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LobbyHubUI : MonoBehaviour
{
    [SerializeField] GameObject messagePrefab;
    [SerializeField] GameObject emojiPrefab;
    [SerializeField] GameObject categoryPrefab;
    [SerializeField] Transform messageListParent;
    [SerializeField] Transform emojiListParent;
    [SerializeField] ScrollRect messageScrollRect;
    [SerializeField] ScrollRect emojiScrollRect;

    Dictionary<string, List<string>> categorizedMessages = new Dictionary<string, List<string>>()
    {
        {
            "Friendly", new List<string> 
            {
            "<anim:wave>YO WAZZA!?</anim>",
            "<anim:wave>WE CHILLIN'</anim>",
            "GOOD LUCK, EVERYONE! <sprite name=\"ManiaMojis 1_13\">",
            "READY POR FAVOR!?!! <sprite name=\"ManiaMojis 1_75\"><sprite name=\"ManiaMojis 1_55\">",
            "BLESS <sprite name=\"ManiaMojis 1_22\">",
            "<sprite name=\"ManiaMojis 1_22\"><sprite name=\"ManiaMojis 1_22\"><sprite name=\"ManiaMojis 1_22\">",
            "HERE’S TO A GREAT GAME! <sprite name=\"ManiaMojis 1_51\"><sprite name=\"ManiaMojis 1_22\">",
            "CAN’T WAIT TO PLAY WITH YOU ALL! <sprite name=\"ManiaMojis 1_18\">",
            "CHEERS YO! <sprite name=\"ManiaMojis 1_19\"><sprite name=\"ManiaMojis 1_13\">",
            "I LOVE  Y'ALL<sprite name=\"ManiaMojis 1_4\">! ",
            "FRIENDS FOREVER! <sprite name=\"ManiaMojis 1_34\">",
            "JK...<sprite name=\"ManiaMojis 1_41\">"
            }   
        },

        { 
            "Competitive", new List<string> 
            {
            "BRING IT ON, NOOBS! <sprite name=\"ManiaMojis 1_16\">",
            "JUST <anim:blink>LEAVE</anim> BRO..",
            "<anim:shake>SUCKERS!!!<sprite name=\"ManiaMojis 1_25\"><sprite name=\"ManiaMojis 1_25\"><sprite name=\"ManiaMojis 1_25\"></anim>",
            "YOU THINK THIS A JOKE? <sprite name=\"ManiaMojis 1_56\">",
            "<anim:shake>PREPARE</anim> TO BE OWNED! <sprite name=\"ManiaMojis 1_54\">",
            "YOUR LOSS IS MY <anim:shake>WARM-UP!</anim> <sprite name=\"ManiaMojis 1_15\"><sprite name=\"ManiaMojis 1_28\">",
            "WE GON FU&K YOU UP!!<sprite name=\"ManiaMojis 1_10\"><sprite name=\"ManiaMojis 1_26\">",
            "I'M <anim:blink>NOT</anim> PLAYING AROUND..",
            "<sprite name=\"ManiaMojis 1_28\"><sprite name=\"ManiaMojis 1_28\"><sprite name=\"ManiaMojis 1_28\">",
            "<anim:wave>YAWN... <sprite name=\"ManiaMojis 1_44\"></anim>",
            "<anim:shake>WAIT AND SEE..</anim>",
            "WHO'S READY TO LOSE? <sprite name=\"ManiaMojis 1_47\">",
            "EASY PEASY! <sprite name=\"ManiaMojis 1_33\">",
            "IMMA KICK UR ASSES.<sprite name=\"ManiaMojis 1_32\">",
            "DIGGING YOUR GRAVE NOW! <sprite name=\"ManiaMojis 1_30\">",
            "MOTHERFU&KERS!",
            "NOOB <anim:blink><color=yellow>ALERT!</anim></color> <sprite name=\"ManiaMojis 1_14\">",
            "READY TO LOSE? <sprite name=\"ManiaMojis 1_11\">"
            }
        }
    };

    List<string> emojis = new List<string>
    {
    "<size=80><sprite name=\"ManiaMojis 1_0\">",
    "<size=80><sprite name=\"ManiaMojis 1_1\">",
    "<sprite name=\"ManiaMojis 1_2\">",
    "<sprite name=\"ManiaMojis 1_3\">",
    "<sprite name=\"ManiaMojis 1_4\">",
    "<sprite name=\"ManiaMojis 1_5\">",
    "<sprite name=\"ManiaMojis 1_6\">",
    "<sprite name=\"ManiaMojis 1_7\">",
    "<sprite name=\"ManiaMojis 1_8\">",
    "<sprite name=\"ManiaMojis 1_9\">",
    "<sprite name=\"ManiaMojis 1_10\">",
    "<sprite name=\"ManiaMojis 1_11\">",
    "<sprite name=\"ManiaMojis 1_12\">",
    "<sprite name=\"ManiaMojis 1_13\">",
    "<sprite name=\"ManiaMojis 1_14\">",
    "<sprite name=\"ManiaMojis 1_15\">",
    "<sprite name=\"ManiaMojis 1_16\">",
    "<sprite name=\"ManiaMojis 1_17\">",
    "<sprite name=\"ManiaMojis 1_18\">",
    "<sprite name=\"ManiaMojis 1_19\">",
    "<sprite name=\"ManiaMojis 1_20\">",
    "<sprite name=\"ManiaMojis 1_21\">",
    "<sprite name=\"ManiaMojis 1_22\">",
    "<sprite name=\"ManiaMojis 1_23\">",
    "<sprite name=\"ManiaMojis 1_24\">",
    "<sprite name=\"ManiaMojis 1_25\">",
    "<sprite name=\"ManiaMojis 1_26\">",
    "<sprite name=\"ManiaMojis 1_27\">",
    "<sprite name=\"ManiaMojis 1_28\">",
    "<sprite name=\"ManiaMojis 1_29\">",
    "<sprite name=\"ManiaMojis 1_30\">",
    "<sprite name=\"ManiaMojis 1_31\">",
    "<sprite name=\"ManiaMojis 1_32\">",
    "<sprite name=\"ManiaMojis 1_33\">",
    "<sprite name=\"ManiaMojis 1_34\">",
    "<sprite name=\"ManiaMojis 1_35\">",
    "<sprite name=\"ManiaMojis 1_36\">",
    "<sprite name=\"ManiaMojis 1_37\">",
    "<sprite name=\"ManiaMojis 1_38\">",
    "<sprite name=\"ManiaMojis 1_39\">",
    "<sprite name=\"ManiaMojis 1_40\">",
    "<sprite name=\"ManiaMojis 1_41\">",
    "<sprite name=\"ManiaMojis 1_42\">",
    "<sprite name=\"ManiaMojis 1_43\">",
    "<sprite name=\"ManiaMojis 1_44\">",
    "<sprite name=\"ManiaMojis 1_45\">",
    "<sprite name=\"ManiaMojis 1_46\">",
    "<sprite name=\"ManiaMojis 1_47\">",
    "<sprite name=\"ManiaMojis 1_48\">",
    "<sprite name=\"ManiaMojis 1_49\">",
    "<sprite name=\"ManiaMojis 1_50\">",
    "<sprite name=\"ManiaMojis 1_51\">",
    "<sprite name=\"ManiaMojis 1_52\">",
    "<sprite name=\"ManiaMojis 1_53\">",
    "<sprite name=\"ManiaMojis 1_54\">",
    "<sprite name=\"ManiaMojis 1_55\">",
    "<sprite name=\"ManiaMojis 1_56\">",
    "<size=80><sprite name=\"ManiaMojis 1_57\">",
    "<size=80><sprite name=\"ManiaMojis 1_58\">",
    "<size=80><sprite name=\"ManiaMojis 1_59\">",
    "<size=80><sprite name=\"ManiaMojis 1_60\">",
    "<sprite name=\"ManiaMojis 1_61\">",
    "<size=80><sprite name=\"ManiaMojis 1_62\">",
    "<size=80><sprite name=\"ManiaMojis 1_63\">",
    "<size=80><sprite name=\"ManiaMojis 1_64\">",
    "<size=85><sprite name=\"ManiaMojis 1_65\">",
    "<size=80><sprite name=\"ManiaMojis 1_66\">",
    "<size=80><sprite name=\"ManiaMojis 1_67\">",
    "<size=85><sprite name=\"ManiaMojis 1_68\">",
    "<size=90><sprite name=\"ManiaMojis 1_69\">",
    "<size=80><sprite name=\"ManiaMojis 1_70\">",
    "<size=80><sprite name=\"ManiaMojis 1_71\">",
    "<size=87><sprite name=\"ManiaMojis 1_72\">",
    "<size=86><sprite name=\"ManiaMojis 1_73\">",
    "<size=80><sprite name=\"ManiaMojis 1_74\">",
    "<size=80><sprite name=\"ManiaMojis 1_75\">"
    };

    void Start()
    {
        UpdateSpriteSizes();
        PopulateMessages();
        PopulateEmojis();
    }
    
    public void AddCategory(string category)
    {
        GameObject newCategory = Instantiate(categoryPrefab, messageListParent);
        TalkBoxCategory categoryHeader = newCategory.GetComponent<TalkBoxCategory>();
        categoryHeader.Initialize(category);

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)messageListParent);
        UpdateContentHeight(messageListParent, categoryPrefab, messageScrollRect);
    }

    public void AddMessage(string message)
    {
        GameObject newMessage = Instantiate(messagePrefab, messageListParent);
        MessageButton messageButton = newMessage.GetComponent<MessageButton>();
        string cleanMsg = message;
        // clean msg before initializing
        Typewriter.CleanText(ref cleanMsg);
        messageButton.Initialize(cleanMsg);
        messageButton.GetComponent<Button>().onClick.AddListener(() => OnMessageClicked(message));

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)messageListParent);
        UpdateContentHeight(messageListParent, messagePrefab, messageScrollRect);
    }

    public void AddEmoji(string emoji)
    {
        GameObject newEmoji = Instantiate(emojiPrefab, emojiListParent);
        MessageButton messageButton = newEmoji.GetComponent<MessageButton>();
        var cleanEmojiMsg = RemoveSizeTag(emoji);
        messageButton.Initialize(cleanEmojiMsg);
        messageButton.GetComponent<Button>().onClick.AddListener(() => OnEmojiClicked(emoji));

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)emojiListParent);
        UpdateGridContentHeight(emojiListParent, emojiPrefab, emojiScrollRect);
    }

    public static string RemoveSizeTag(string input)
    {
        string pattern = @"<size=\d+>";
        return System.Text.RegularExpressions.Regex.Replace(input, pattern, string.Empty);
    }

    void PopulateMessages()
    {
        foreach (var category in categorizedMessages)
        {
            AddCategory(category.Key);
            foreach (var msg in category.Value)
            {
                AddMessage(msg);
            }
        }
    }

    void PopulateEmojis()
    {
        foreach (var emoji in emojis)
        {
            AddEmoji(emoji);
        }
    }

    void UpdateSpriteSizes()
    {
        foreach (var category in categorizedMessages.Keys.ToList())
        {
            for (int i = 0; i < categorizedMessages[category].Count; i++)
            {
                categorizedMessages[category][i] = InsertSizeTag(categorizedMessages[category][i]);
            }
        }
    }

    string InsertSizeTag(string message)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            message,
            @"<sprite",
            "<size=36><sprite"
        );
    }

    void UpdateGridContentHeight(Transform listParent, GameObject prefab, ScrollRect scrollRect)
    {
        RectTransform contentRectTransform = listParent.GetComponent<RectTransform>();
        GridLayoutGroup gridLayoutGroup = listParent.GetComponent<GridLayoutGroup>();

        if (gridLayoutGroup == null)
        {
            Debug.LogError("GridLayoutGroup is not attached to list parent.");
            return;
        }

        float itemHeight = gridLayoutGroup.cellSize.y;
        float spacingY = gridLayoutGroup.spacing.y;
        int columnCount = Mathf.Max(1, Mathf.FloorToInt((contentRectTransform.rect.width + gridLayoutGroup.spacing.x) / (gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x)));

        int rowCount = Mathf.CeilToInt((float)listParent.childCount / columnCount);
        float totalHeight = (itemHeight * rowCount) + (spacingY * (rowCount - 1));

        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalHeight);

        Canvas.ForceUpdateCanvases();
    }

    void UpdateContentHeight(Transform listParent, GameObject prefab, ScrollRect scrollRect)
    {
        RectTransform contentRectTransform = listParent.GetComponent<RectTransform>();
        float itemHeight = prefab.GetComponent<RectTransform>().rect.height;
        float spacing = listParent.GetComponent<VerticalLayoutGroup>().spacing;
        float totalHeight = (itemHeight + spacing) * listParent.childCount;

        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalHeight);

        Canvas.ForceUpdateCanvases();
    }

    void OnMessageClicked(string message)
    {
        var player = FusionLauncher.Instance.Runner  ().LocalPlayer;
        if (player != PlayerRef.None)
        {
            PublicLobbyManager.Instance.RpcShowMessage(player, message);
        }
        else
        {
            Debug.LogError("Local player reference is invalid!");
        }
    }

    void OnEmojiClicked(string emoji)
    {
        var player = FusionLauncher.Instance.Runner().LocalPlayer;
        if (player != PlayerRef.None)
        {
            PublicLobbyManager.Instance.RpcShowEmote(player, emoji);
        }
        else
        {
            Debug.LogError("Local player reference is invalid!");
        }
    }
}
