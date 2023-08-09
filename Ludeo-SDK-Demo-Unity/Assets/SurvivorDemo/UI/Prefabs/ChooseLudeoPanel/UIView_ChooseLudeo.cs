using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class UIView_ChooseLudeo : MonoBehaviour
{
    public Button Play;
    public Button Cancel;
    public TMP_InputField LudeoGUID;
    public TMP_Dropdown PreviousLudeos;

    private void Awake()
    {
        InitializeDropdown();
        LudeoGUID.onValueChanged.AddListener(InputGuidChanged);
        Cancel.onClick.AddListener(() => gameObject.SetActive(false));
        PreviousLudeos.onValueChanged.AddListener(i => LudeoGUID.text = PreviousLudeos.options[i].text);
        PreviousLudeos.onValueChanged.AddListener(i => InputGuidChanged(PreviousLudeos.options[i].text));
    }

    public void Init(Action<string> play)
    {
        Play.onClick.RemoveAllListeners();
        Play.onClick.AddListener(() => play(LudeoGUID.text));
    }

    private void InputGuidChanged(string guid)
    {
        // xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
        var split = guid.Split('-');

        if (string.IsNullOrEmpty(guid))
        {
            Play.interactable = false;
            return;
        }
        if (split.Length != 5)
        {
            Play.interactable = false;
            return;
        }
        if (guid.Length != 36)
        {
            Play.interactable = false;
            return;
        }
        if (split[0].Length != 8)
        {
            Play.interactable = false;
            return;
        }
        if (split[1].Length != 4)
        {
            Play.interactable = false;
            return;
        }
        if (split[2].Length != 4)
        {
            Play.interactable = false;
            return;
        }
        if (split[3].Length != 4)
        {
            Play.interactable = false;
            return;
        }
        if (split[4].Length != 12)
        {
            Play.interactable = false;
            return;
        }

        SaveLudeo(guid);
        Play.interactable = true;
    }

    private void SaveLudeo(string guid)
    {
        var arr = JsonConvert.DeserializeObject<List<string>>
            (PlayerPrefs.GetString("Saved_Ludeos", "[]"));

        if (arr == null || arr.Count == 0)
        {
            arr = new List<string> { guid };
            PlayerPrefs.SetString("Saved_Ludeos", JsonConvert.SerializeObject(arr));
        }
        else
        {
            if (!arr.Contains(guid))
            {
                arr.Add(guid);
                PlayerPrefs.SetString("Saved_Ludeos", JsonConvert.SerializeObject(arr));
            }
        }
    }

    private void InitializeDropdown()
    {
        PreviousLudeos.ClearOptions();
        var ludeosPrefs = PlayerPrefs.GetString("Saved_Ludeos");
        var ludeos = JsonConvert.DeserializeObject<string[]>(ludeosPrefs);

        if (ludeos != null)
        {
            PreviousLudeos.AddOptions(ludeos.ToList());
            PreviousLudeos.value = ludeos.Length - 1;
            LudeoGUID.text = ludeos[ludeos.Length-1];
        }
    }

#if UNITY_EDITOR
    [MenuItem("Ludeo/Clear Saved Ludeos")]
    public static void ClearSavedLudeos()
    {
        PlayerPrefs.SetString("Saved_Ludeos", "[]");
    }
#endif

}
