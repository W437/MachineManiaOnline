using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using System;

public class GoogleSheetsExample : MonoBehaviour
{
    private static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
    private static readonly string ApplicationName = "Pickup Balance";

    private SkillData skillData;

    void Start()
    {
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "trans-trees-425110-g1-06c684a9a9d4.json");

        GoogleCredential credential;

        using (var stream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }

        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        String spreadsheetId = "11NDkrOoQljZAib_2CIjTJh2prgTpKJJ0bx9l1Mc3mlg";

        skillData = new SkillData
        {
            SkillNames = new List<string>(),
            Durations = new List<int>(),
            DropRates = new List<float>()
        };

        List<string> stringRange = new List<string> { "SkillSheet!A2:A21" };
        List<string> intRange = new List<string> { "SkillSheet!C2:C21" };
        List<string> floatRange = new List<string> { "SkillSheet!E2:E21" };

        FetchAndStoreData(service, spreadsheetId, stringRange, ref skillData.SkillNames, DataType.String);
        FetchAndStoreData(service, spreadsheetId, intRange, ref skillData.Durations, DataType.Int);
        FetchAndStoreData(service, spreadsheetId, floatRange, ref skillData.DropRates, DataType.FloatPercent);

        SaveDataToJson(skillData);

        Debug.Log("Data saved to JSON file.");
    }

    private void FetchAndStoreData<T>(SheetsService service, string spreadsheetId, List<string> ranges, ref List<T> dataList, DataType dataType)
    {
        foreach (var range in ranges)
        {
            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = request.Execute();
            IList<IList<object>> values = response.Values;

            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    try
                    {
                        object cellValue = row[0];
                        switch (dataType)
                        {
                            case DataType.String:
                                dataList.Add((T)Convert.ChangeType(cellValue.ToString(), typeof(T)));
                                break;
                            case DataType.Int:
                                dataList.Add((T)Convert.ChangeType(Convert.ToInt32(cellValue), typeof(T)));
                                break;
                            case DataType.Float:
                                dataList.Add((T)Convert.ChangeType(Convert.ToSingle(cellValue), typeof(T)));
                                break;
                            case DataType.FloatPercent:
                                string cellValueString = cellValue.ToString().Replace("%", "");
                                float floatValue = float.Parse(cellValueString) / 100f;
                                dataList.Add((T)Convert.ChangeType(floatValue, typeof(T)));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error converting value: {row[0]} - {ex.Message}");
                    }
                }
            }
            else
            {
                Debug.Log($"No data found at range: {range}.");
            }
        }
    }

    private void SaveDataToJson(SkillData data)
    {
        string jsonPath = Path.Combine(Application.persistentDataPath, "SkillData.json");
        string json = JsonConvert.SerializeObject(new SkillDataWrapper { SkillData = data }, Formatting.Indented);
        File.WriteAllText(jsonPath, json);
    }

    private enum DataType
    {
        String,
        Int,
        Float,
        FloatPercent
    }
}
