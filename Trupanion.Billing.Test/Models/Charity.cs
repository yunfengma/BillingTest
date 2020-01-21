//----------------------------------------------------------------------------------------------------------
// <copyright file="Charity.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace Trupanion.Billing.Test.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Charity
    {
        public static CharityIds FarleyFoundation { get; set; }
        public static CharityIds BCSPCABiscuitFund { get; set; }
        public static CharityIds AmericanHumaneAssociation { get; set; }
        public static CharityIds RedRover { get; set; }
        public static CharityIds BestFriendsAnimalSocietyNoMoreHomelessPets { get; set; }
        public static CharityIds SpeakingofDogs { get; set; }
        public static CharityIds SeattleHumane { get; set; }
        public static CharityIds Paws4People { get; set; }

        public static List<CharityIds> Charities { get; set; }

        public static void Init()
        {
            Charities = new List<CharityIds>();

            // CA
            FarleyFoundation = new CharityIds();
            FarleyFoundation.Name = "Farley Foundation";
            FarleyFoundation.Id = 2;
            FarleyFoundation.UniqueId = Guid.Parse("D9A5EB0B-5306-40E3-ADCD-794B413F75A3");
            Charities.Add(FarleyFoundation);

            BCSPCABiscuitFund = new CharityIds();
            BCSPCABiscuitFund.Name = "BC SPCA Biscuit Fund";
            BCSPCABiscuitFund.Id = 3;
            BCSPCABiscuitFund.UniqueId = Guid.Parse("1D4EBBBB-1B68-434C-B9E5-8D3423D40A0C");
            Charities.Add(BCSPCABiscuitFund);

            SpeakingofDogs = new CharityIds();
            SpeakingofDogs.Name = "Speaking of Dogs";
            SpeakingofDogs.Id = 12;
            SpeakingofDogs.UniqueId = Guid.Parse("9372BF9B-E3F4-43D8-8366-9A273AE5158A");
            Charities.Add(SpeakingofDogs);

            // US
            AmericanHumaneAssociation = new CharityIds();
            AmericanHumaneAssociation.Name = "American Humane Association";
            AmericanHumaneAssociation.Id = 8;
            AmericanHumaneAssociation.UniqueId = Guid.Parse("6FA5C496-20A1-41A2-B690-E0EFA3149EBA");
            Charities.Add(AmericanHumaneAssociation);

            RedRover = new CharityIds();
            RedRover.Name = "RedRover";
            RedRover.Id = 10;
            RedRover.UniqueId = Guid.Parse("78C5D463-3859-4AA6-B910-14C68F62DC3A");
            Charities.Add(RedRover);

            BestFriendsAnimalSocietyNoMoreHomelessPets = new CharityIds();
            BestFriendsAnimalSocietyNoMoreHomelessPets.Name = "Best Friends Animal Society No More Homeless Pets";
            BestFriendsAnimalSocietyNoMoreHomelessPets.Id = 11;
            BestFriendsAnimalSocietyNoMoreHomelessPets.UniqueId = Guid.Parse("9037BC8A-B526-400E-A9B9-02B67C7DB5F9");
            Charities.Add(BestFriendsAnimalSocietyNoMoreHomelessPets);

            SeattleHumane = new CharityIds();
            SeattleHumane.Name = "Seattle Humane";
            SeattleHumane.Id = 13;
            SeattleHumane.UniqueId = Guid.Parse("30C2B29A-07CC-4359-94E6-E99E2B0CF200");
            Charities.Add(SeattleHumane);

            Paws4People = new CharityIds();
            Paws4People.Name = "Paws 4 People";
            Paws4People.Id = 14;
            Paws4People.UniqueId = Guid.Parse("F95D059E-340C-4049-AB5E-2F2830592E07");
            Charities.Add(Paws4People);
        }

        public static Guid GetUniqueIdFromId(int id)
        {
            CharityIds ch = Charities.Where(i => i.Id == id).First();
            return ch == null ? Guid.Empty : ch.UniqueId;
        }
    }
}
