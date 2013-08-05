﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

using DeltaCommon.Component;

namespace DeltaCommon.Astar
{
    public class Link
    {
        Node mStartNode;
        Node mEndNode;
        float mCost;
        int mLinkId;
        string mLinkDescription;
        

        #region Properties

     
        public Node StartNode
        {
            get
            {
                return mStartNode;
            }
            set
            {
                mStartNode = value;
                mStartNode.OutLinks.Add(this);
            }
        }


        public Node EndNode
        {
            get
            {
                return mEndNode;
            }
            set
            {
                mEndNode = value;
                mEndNode.InLinks.Add(this);
            }
        }

        public float Cost
        {
            get
            {
                return mCost;
            }
            set
            {
                mCost = value;
            }
        }
        public int LinkId 
        {
            get
            {
                return mLinkId;
            }
            set
            {
                mLinkId = value;
            }
        }

        public string LinkDescription
        {
            get
            {
                mLinkDescription = "Link ID:" + mLinkId + " StartNode ID: " + StartNode.NodeId + " EndNode ID : " + EndNode.NodeId + " Cost: X:" + mCost;
                return mLinkDescription;
            }
        }

        #endregion
        public Link(Node startNode, Node endNode, float cost) 
        {
            StartNode = startNode;
            EndNode = endNode;
            mCost = cost;
         }

    }
}
