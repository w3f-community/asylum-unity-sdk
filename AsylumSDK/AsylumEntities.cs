using System;
using System.Collections.Generic;

namespace Asylum
{
    #region PARSING CLASSES
    //MB rename classes used in the data parsing to Wrapped* f.e. GameMetadata => WrappedGameGameMetadata

    [Serializable]
    public class GameMetadata
    {
        public string id;
        public string title;
        public string img;
        public string genre;
        public string shortDescription;
        public string description;
        public string[] gallery;
        public Review[] reviews;
    }

    [Serializable]
    public class Review
    {
        public string id;
        public string name;
        public string text;
        public string date;
        public int rating;
        public string address;
    }

    [Serializable]
    public class AsylumTemplate
    {
        public string id;
        public string name;
        public string max;
        public string metadata;
        public string issuer;
        public int nftCount;
    }

    [Serializable]
    public class Owner
    {
        public string AccountId;
    }

    [Serializable]
    public class AsylumItem
    {
        public string id;
        public string templateId;
        public Owner owner;
        public string recipient;
        public string royalty;
        public string metadata;
        public bool equipped;
        public Interpretation[] interpretations;
    }

    [Serializable]
    public class InterpretationInfo
    {
        public string id;
        public string src;
        public string metadata;
    }

    [Serializable]
    public class Interpretation
    {
        public string[] tags;
        public InterpretationInfo interpretation;
    }

    #endregion


    public class InterpretationCombineID
    {
        public string templateID;
        public string interpretationID; //Can dublicate in differnt templates

        public override bool Equals(object obj)
        {
            return obj is InterpretationCombineID iD &&
                   templateID == iD.templateID &&
                   interpretationID == iD.interpretationID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(templateID, interpretationID);
        }
    }
}
