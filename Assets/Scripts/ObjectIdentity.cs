using UnityEngine;

public enum ObjectIdentityCategory
{
    Celestial,
    ManMade
}

public class ObjectIdentity : MonoBehaviour
{
    public string displayName;
    public ObjectIdentityCategory category = ObjectIdentityCategory.ManMade;

    public string HoverName
    {
        get
        {
            return string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;
        }
    }

    public void Configure(string name, ObjectIdentityCategory identityCategory)
    {
        displayName = name;
        category = identityCategory;
        gameObject.name = name;
    }
}
