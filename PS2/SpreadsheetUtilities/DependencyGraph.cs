// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Completed By: Bryan Hatasaka
/// u1028471
/// </summary>
namespace SpreadsheetUtilities
{

    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        Dictionary<int, HashSet<String>> dependentsDictionary;
        Dictionary<int, HashSet<String>> dependeesDictionary;
        private int p_size;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            dependentsDictionary = new Dictionary<int, HashSet<String>>();
            dependeesDictionary = new Dictionary<int, HashSet<String>>();
            p_size = 0;
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return p_size; }
        }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
            get
            {
                int sKey = s.GetHashCode();
                if (dependeesDictionary.ContainsKey(sKey))
                    return dependeesDictionary[sKey].Count;

                return 0;
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            int sKey = s.GetHashCode();
            if (dependentsDictionary.ContainsKey(sKey))
                return true;

            return false;
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            int sKey = s.GetHashCode();
            if (dependeesDictionary.ContainsKey(sKey))
                return true;

            return false;
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            int sKey = s.GetHashCode();
            if(dependentsDictionary.ContainsKey(sKey))
            {
                return dependentsDictionary[sKey].ToArray<String>();
            }
            
            //If s doesn't have any dependents, then return an empty list
            return new List<String>();
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            int sKey = s.GetHashCode();
            if (dependeesDictionary.ContainsKey(sKey))
            {
                return dependeesDictionary[sKey].ToArray<String>();
            }

            //If s doesn't have any dependees, then return an empty list
            return new List<String>();
        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>        /// 
        public void AddDependency(string s, string t)
        {
            int sKey = s.GetHashCode();
            int tKey = t.GetHashCode();
            bool dependentAddSucess;
            bool dependeeAddSucess;

            //Add the dependent t to s if s exists
            if(dependentsDictionary.ContainsKey(sKey))
                dependentAddSucess = dependentsDictionary[sKey].Add(t);
            else
            {
                //Add the dependent t to a new set and store it with s
                HashSet<String> dependentsSet = new HashSet<string>() { t };
                dependentsDictionary.Add(sKey, dependentsSet);
                dependentAddSucess = true;
            }

            //Add the dependee s to t if t exists
            if (dependeesDictionary.ContainsKey(tKey))
                dependeeAddSucess = dependeesDictionary[tKey].Add(s);
            else
            {
                //Add the dependee s to a new set and store it with t
                HashSet<String> dependeeSet = new HashSet<string>() { s };
                dependeesDictionary.Add(tKey, dependeeSet);
                dependeeAddSucess = true;
            }

            //Verifies that the numbers were not already in the set before
            //incrementing the size
            if (dependentAddSucess && dependeeAddSucess)
                p_size++;
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            int sKey = s.GetHashCode();
            int tKey = t.GetHashCode();

            if (DictionariesHaveKeys(sKey, tKey))
            {
                bool dependentRemoveSucess;
                bool dependeeRemoveSucess;

                dependentRemoveSucess = dependentsDictionary[sKey].Remove(t);
                dependeeRemoveSucess = dependeesDictionary[tKey].Remove(s);

                if(dependentRemoveSucess && dependeeRemoveSucess)
                    p_size--;

                //Checks to see if the term now has no dependents/dependees
                //and if so, remove it from the appropriate dictionary
                if (dependentsDictionary[sKey].Count == 0)
                {
                    dependentsDictionary.Remove(sKey);
                }
                if (dependeesDictionary[tKey].Count == 0)
                {
                    dependeesDictionary.Remove(tKey);
                }
            }
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            int sKey = s.GetHashCode();

            if (dependentsDictionary.ContainsKey(sKey))
            {
                //Remove s as the dependee from all of s's dependents
                HashSet<String> set = new HashSet<String>(dependentsDictionary[sKey]);
                foreach(String dependent in set)
                {
                    this.RemoveDependency(s, dependent);
                }
            }

            //Add all of the dependents to s
            foreach(string dependent in newDependents)
            {
                this.AddDependency(s, dependent);
            }
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            int sKey = s.GetHashCode();

            if (dependeesDictionary.ContainsKey(sKey))
            {
                //Remove s as the dependent from all of s's dependees
                HashSet<String> dependees = new HashSet<string>(dependeesDictionary[sKey]);
                foreach(String dependee in dependees)
                {
                    RemoveDependency(dependee, s);
                }
            }

            //Add s as the dependent to all of the new dependees
            foreach (string dependee in newDependees)
            {
                this.AddDependency(dependee, s);
            }
        }
        
        /// <summary>
        /// Checks if the dependentsDictionary contains the sKey
        /// and if the dependeesDictionary contains the tKey.
        /// 
        /// Returns true if both dictionaries contain the keys.
        /// False otherwise.
        /// </summary>
        /// <param name="sKey"></param>
        /// <param name="tKey"></param>
        /// <returns></returns>
        private bool DictionariesHaveKeys(int sKey, int tKey)
        {
            if (dependentsDictionary.ContainsKey(sKey) && dependeesDictionary.ContainsKey(tKey))
            {
                return true;
            }
            return false;
        }
    }
}