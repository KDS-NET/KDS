/*
 *    KDS .NET - A KDS algorithm simulator for .NET
 *
 *    (C) 2021, LaBRI - Laboratoire Bordelais de Recherche en Informatique
 *                      (Bordeaux's Computer Science Research Laboratory)
 *    (C) 2021, Gustave Monce
 *
 *    This library is free software; you can redistribute it and/or
 *    modify it under the terms of the GNU Lesser General Public
 *    License as published by the Free Software Foundation;
 *    version 2.1 of the License.
 *
 *    This library is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *    Lesser General Public License for more details.
 */
using System.Collections.Generic;
using System.Linq;

namespace KDS
{
    public static class Utils
    {
        // from: https://stackoverflow.com/questions/36701657/comparing-two-lists-to-find-added-or-removed-element-in-one-of-them
        public static (HashSet<T> added, HashSet<T> removed, HashSet<T> same) CompareLists<T>(IEnumerable<T> oldList, IEnumerable<T> newList)
        {
            HashSet<T> added = new();
            HashSet<T> removed = new();
            HashSet<T> same = new();

            int i = 0;
            int j = 0;

            while (i < oldList.Count() || j < newList.Count())
            {
                if (i >= oldList.Count())
                {
                    for (; j < newList.Count(); j++)
                    {
                        added.Add(newList.ElementAt(j));
                    }
                    break;
                }
                if (j >= newList.Count())
                {
                    for (; i < oldList.Count(); i++)
                    {
                        removed.Add(oldList.ElementAt(i));
                    }
                    break;
                }

                if (oldList.ElementAt(i).Equals(newList.ElementAt(j)))
                {
                    same.Add(oldList.ElementAt(i));
                    i++;
                    j++;
                }
                else if (j < (newList.Count() - 1) && oldList.ElementAt(i).Equals(newList.ElementAt(j + 1)))
                {
                    added.Add(newList.ElementAt(j));
                    j++;
                }
                else
                {
                    removed.Add(oldList.ElementAt(i));
                    i++;
                }
            }

            return (added, removed, same);
        }
    }
}
