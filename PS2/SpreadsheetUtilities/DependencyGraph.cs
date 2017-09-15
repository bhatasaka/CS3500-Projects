﻿// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                HashSet<String> setCopy = dependentsDictionary[sKey];

                return setCopy;
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
                HashSet<String> setCopy = dependeesDictionary[sKey];

                return setCopy;
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
            if(DictionariesHaveKeys(sKey, tKey))
            {
                p_size++;
                 //Add the dependent t to s
                dependentsDictionary[sKey].Add(t);

                //Add the dependee s to t
                dependeesDictionary[tKey].Add(s);
            }
            else
            {
                //Add the dependent t to a new set and store it with s
                HashSet<String> dependentsSet = new HashSet<string>() { t };
                dependentsDictionary.Add(sKey, dependentsSet);

                //Add the dependee s to a new set and store it with t
                HashSet<String> dependeeSet = new HashSet<string>() { s };
                dependeesDictionary.Add(tKey, dependeeSet);
            }
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
                dependentsDictionary[sKey].Remove(t);
                dependeesDictionary[tKey].Remove(s);

                if(dependentsDictionary[sKey].Count == 0)
                {
                    dependentsDictionary.Remove(sKey);
                }
                if(dependeesDictionary[tKey].Count == 0)
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
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
        }

        private bool DictionariesHaveKeys(int sKey, int tKey)
        {
            if (dependentsDictionary.ContainsKey(sKey) && dependeesDictionary.ContainsKey(tKey))
            {
                return true;
            }
            else return false;
        }
    }
}