using UnityEngine;
using System;

[System.Serializable]
public class PlayerData
{
    [Header("Basic Info")]
    public string nickname;
    public Gender gender;
    public DateTime creationDate;
    public DateTime lastPlayDate;
    public float playTime;

    [Header("Appearance")]
    public int skinColorIndex;
    public int hairStyleIndex;
    public int hairColorIndex;
    public int eyeColorIndex;
    public int outfitIndex;

    [Header("Game Progress")]
    public int currentLevel;
    public int experience;
    public int money;
    public string currentSceneName;
    public Vector3 currentPosition;

    [Header("Interview Progress")]
    public int interviewScore;
    public int completedInterviews;
    public string[] unlockedDialogues;

    public PlayerData()
    {
        nickname = "";
        gender = Gender.Male;
        creationDate = DateTime.Now;
        lastPlayDate = DateTime.Now;
        playTime = 0f;

        skinColorIndex = 0;
        hairStyleIndex = 0;
        hairColorIndex = 0;
        eyeColorIndex = 0;
        outfitIndex = 0;

        currentLevel = 1;
        experience = 0;
        money = 1000;
        currentSceneName = "Intro";
        currentPosition = Vector3.zero;

        interviewScore = 0;
        completedInterviews = 0;
        unlockedDialogues = new string[0];
    }

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(nickname);
    }

    public string GetDisplayInfo()
    {
        if (IsEmpty())
            return "ºó ½½·Ô";

        return $"{nickname}\nLv.{currentLevel}\n{lastPlayDate.ToString("yyyy/MM/dd HH:mm")}";
    }
}

public enum Gender
{
    Male,
    Female
}