using System.Collections.Generic;
using System;

[Serializable]
public class SkillData
{
    public List<string> SkillNames;
    public List<int> Durations;
    public List<float> DropRates;
}

[Serializable]
public class SkillDataWrapper
{
    public SkillData SkillData;
}