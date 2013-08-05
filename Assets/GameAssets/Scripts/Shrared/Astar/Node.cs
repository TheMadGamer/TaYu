using System;
using System.Collections.Generic;
using System.Diagnostics;

using DeltaCommon.Component;

namespace DeltaCommon.Astar
{
    public class Node: IComparable<Node>
    {
        //Position 
        Vec2 mPosition;

        //
        int mNodeId;

        //cost of the node
        float mF=0;
        
        //cost of the already existing path from origin
        float mG=0;
        
        //estimate of the cost to destination
        float mH=0;
        

        //Parent
        Node mParent=null;
        
        //Node Description
        string mNodeDescription;

        List<Link> mInLinks=new List<Link>();
        List<Link> mOutLinks = new List<Link>();

        #region Properties

        public Vec2 Position
        {
            get
            {
                return mPosition;
            }
            set
            {
                mPosition = value;
            }
        }

        public int NodeId
        {
            get
            {
                return mNodeId;
            }
            set
            {
                mNodeId = value;
            }
        }

        public float F
        {
            get
            {
                return mF;
            }
            set
            {
                mF = value;
            }
        }

        public float G
        {
            get
            {
                return mG;
            }
            set
            {
                mG = value;
            }
        }

        public float H
        {
            get
            {
                return mH;
            }
            set
            {
                mH = value;
            }
        }
        public Node Parent
        {
            get
            {
                return mParent;
            }
            set
            {
                mParent = value;
            }
        }
        public List<Link> InLinks
        {
            get
            {
                return mInLinks;
            }
        }
        public List<Link> OutLinks
        {
            get
            {
                return mOutLinks;
            }
        }
        public string NodeDescription
        {
            get
            {
                if (mParent != null)
                {
                    mNodeDescription = "Node Point ID: " + mNodeId + " X " + mPosition.X + " Y " + mPosition.Y + " F " + mF + " G " + mG + " H " + mH + " Parent X " + mParent.Position.X + " Parent Y " + mParent.Position.Y;
                }
                else
                {
                    mNodeDescription = "Node Point ID:" + mNodeId + " X " + mPosition.X + " Y " + mPosition.Y + " F " + mF + " G " + mG + " H " + mH + " No Parent";
                }
                if (mOutLinks.Count > 0)
                {
                    mNodeDescription += " Outgoing Links";
                    foreach (Link k in mOutLinks)
                    {
                        mNodeDescription += " ID:" + k.LinkId;
                    }
                }
                return mNodeDescription;
            }

        }
        #endregion

        public Node(Vec2 position)
        {
            mPosition = position;
        }

        public Node(Node node)
        {
            mPosition = node.Position;
            mInLinks = node.InLinks;
            mOutLinks = node.OutLinks;
            mNodeId = node.NodeId;
        }

     /*
        public Node FromList(List<Node> listNode)
        {
            foreach (Node n in listNode)
            {
                if (mPosition == n.Position)
                {
                    return n;
                }
            }
            return null;
        }
        */
        
        public int CompareTo(Node node)
        {
            return mF.CompareTo(node.mF);
        }

      
        public override bool Equals(object O)
        {
            Node N = (Node)O;
            if (N == null) throw new ArgumentException("Type " + O.GetType() + " cannot be compared with type " + GetType() + " !");
            return Position.Equals(N.Position);
        }

        public override int GetHashCode()
        {
            return this.Position.GetHashCode();
        }


        
       

    }
}
