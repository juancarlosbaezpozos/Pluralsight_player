using System;
using System.Collections.Generic;
using System.Linq;

namespace Pluralsight.Domain;

public class UserSubscription
{
    public LibrarySubscription LibrarySubscription;

    public List<Slice> Slices;

    public bool IsActiveSliceSubscription()
    {
        if (LibrarySubscription == null || LibrarySubscription.IsExpired)
        {
            return Slices.Any((Slice x) => x.Expires > DateTimeOffset.UtcNow);
        }
        return false;
    }
}
