#nullable enable

using CodeNav.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeNav.Shared.Comparers
{
    public class CodeItemComparer : IEqualityComparer<CodeItem>
    {
        public bool Equals(CodeItem left, CodeItem right)
        {
            //Check whether the objects are the same object. 
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            //Check whether the products' properties are equal. 
            var membersAreEqual = true;
            if (left is CodeClassItem leftItem && 
                right is CodeClassItem rightItem)
            {
                membersAreEqual = leftItem.Members.SequenceEqual(rightItem.Members, new CodeItemComparer());
            }
            if (left is CodeNamespaceItem leftNamespaceItem &&
                right is CodeNamespaceItem rightNamespaceItem)
            {
                membersAreEqual = leftNamespaceItem.Members.SequenceEqual(rightNamespaceItem.Members, new CodeItemComparer());
            }

            return left != null && right != null && left.Id.Equals(right.Id) && membersAreEqual;
        }

        // Not used, but must be implemented because of interface
        public int GetHashCode(CodeItem obj)
        {
            return 0;
        }
    }
}
