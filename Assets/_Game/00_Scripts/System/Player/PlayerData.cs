using System;
using UnityEngine;

namespace Slafurry.System.Player
{
    public enum Gender
    {
        Boy = 0,
        Girl = 1
    }

    public static class PlayerData
    {
        private const string GenderKey = "PlayerGender";

        public static Gender CurrentGender
        {
            get => (Gender)PlayerPrefs.GetInt(GenderKey, 0);
            set
            {
                PlayerPrefs.SetInt(GenderKey, (int)value);
                PlayerPrefs.Save();
                OnGenderChanged?.Invoke(value);
            }
        }

        public static bool IsBoy => CurrentGender == Gender.Boy;
        public static bool IsGirl => CurrentGender == Gender.Girl;

        public static event Action<Gender> OnGenderChanged;

        public static void SetGender(Gender gender) => CurrentGender = gender;
        public static void SetBoy() => CurrentGender = Gender.Boy;
        public static void SetGirl() => CurrentGender = Gender.Girl;
    }
}
