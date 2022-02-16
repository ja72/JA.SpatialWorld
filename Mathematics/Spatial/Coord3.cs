using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace JA.Mathematics.Spatial
{
    using num = Double;
    using vec3 = Vector3;
    using mat3 = Matrix3;
    using qua3 = Quaternion;
    using CS3 = Coord3;


    [DebuggerDisplay("ID={ID} Parent={ParentID} Pos={Position} Orient={Orientation}")]
    public class Coord3 : Transform
    {
        readonly List<CS3> m_children;
        int m_level;
        readonly int m_id;
        string m_name;
        static int last_id = 0;

        #region Factory

        private Coord3() : base(Transform.I)
        {
            this.m_name = "Ground";
#pragma warning disable S3010 // Static fields should not be updated in constructors
            this.m_id = last_id++;
#pragma warning restore S3010 // Static fields should not be updated in constructors
            this.m_level = 0;
            this.Parent = null;
            this.m_children = new List<Coord3>();
        }

        public Coord3(CS3 parent, vec3 pos) : this(null, parent, pos) { }
        public Coord3(CS3 parent, mat3 orient) : this(null, vec3.Zero, orient) { }
        public Coord3(CS3 parent, vec3 pos, mat3 orient) : this(null, parent, pos, orient ) { }
        public Coord3(CS3 parent, qua3 orient) : this(null, vec3.Zero, orient) { }
        public Coord3(CS3 parent, vec3 pos, qua3 orient) : this(null, parent, pos, orient) { }
        public Coord3(CS3 parent, vec3 pos, AxisName axis, num angle) : this(null, parent, pos, axis, angle) { }
        public Coord3(CS3 parent, vec3 pos, vec3 axis, num angle) : this(null, parent, pos, axis, angle) { }
        public Coord3(string name, CS3 parent, vec3 pos) : base(pos)
        {
            this.m_name = name;
#pragma warning disable S3010 // Static fields should not be updated in constructors
            this.m_id = last_id++;
#pragma warning restore S3010 // Static fields should not be updated in constructors
            this.m_children = new List<Coord3>();
            AttachTo(parent);
        }

        public Coord3(string name, CS3 parent, mat3 orient) : this(name, parent, vec3.Zero, orient) { }

        public Coord3(string name, CS3 parent, vec3 pos, mat3 orient) : base(pos, orient)
        {
            this.m_name = name;
#pragma warning disable S3010 // Static fields should not be updated in constructors
            this.m_id = last_id++;
#pragma warning restore S3010 // Static fields should not be updated in constructors
            this.m_children = new List<Coord3>();
            AttachTo(parent);
        }
        public Coord3(string name, CS3 parent, vec3 pos, qua3 orient)
            : base(pos, orient)
        {
            this.m_name = name;
#pragma warning disable S3010 // Static fields should not be updated in constructors
            this.m_id = last_id++;
#pragma warning restore S3010 // Static fields should not be updated in constructors
            this.m_children = new List<Coord3>();
            AttachTo(parent);
        }
        public Coord3(string name, CS3 parent, vec3 pos, AxisName axis, num angle)
            : base(pos, axis, angle)
        {
            this.m_name = name;
#pragma warning disable S3010 // Static fields should not be updated in constructors
            this.m_id = last_id++;
#pragma warning restore S3010 // Static fields should not be updated in constructors
            this.m_children = new List<Coord3>();
            AttachTo(parent);
        }
        public Coord3(string name, CS3 parent, vec3 pos, vec3 axis, num angle)
            : base(pos, axis, angle)
        {
            this.m_name = name;
#pragma warning disable S3010 // Static fields should not be updated in constructors
            this.m_id = last_id++;
#pragma warning restore S3010 // Static fields should not be updated in constructors
            this.m_children = new List<Coord3>();
            AttachTo(parent);
        }
        public Coord3(CS3 parent, Transform transform) : this(null, parent, transform) { }
        public Coord3(string name, CS3 parent, Transform transform)
            : base(transform)
        {
            this.m_name = name;
#pragma warning disable S3010 // Static fields should not be updated in constructors
            this.m_id = last_id++;
#pragma warning restore S3010 // Static fields should not be updated in constructors
            this.m_children = new List<Coord3>();
            AttachTo(parent);
        }
        #endregion

        #region Functions/Actions
        /// <summary>
        /// Detaches from parent and becomes a root
        /// </summary>
        public void Detach()
        {
            if( Parent != null )
            {
                if( Parent.m_children.IndexOf(this) != -1 )
                {
                    Parent.m_children.Remove(this);
                }
            }
            Parent = null;
        }

        /// <summary>
        /// Keeps local position constant, and attaches coordinate system to new parent
        /// </summary>
        /// <param name="parent">The coordinate system to attach to</param>
        public void AttachTo(CS3 parent)
        {
            Detach();
            if( parent != null )
            {
                Parent = parent;
                m_level = parent.m_level + 1;
                if( Parent.m_children.IndexOf(this) == -1 )
                {
                    Parent.m_children.Add(this);
                }
            }
        }

        /// <summary>
        /// Keeps global position constant, and attaches coordinate system to new parent
        /// </summary>
        /// <param name="parent">The coordinate system to link to</param>
        public void LinkTo(CS3 parent)
        {
            vec3 r = GlobalPosition;
            mat3 E = GlobalRotation;
            Position = parent.Global2Local(r);
            Rotation = parent.Global2Local(E);
            AttachTo(parent);
        }


        /// <summary>
        /// Creates a new coordinate system with the same global position, but attached to a different parent
        /// </summary>
        /// <param name="parent">The coordinate system to link to</param>
        /// <returns>New coordinate system linked to <paramref name="parent"/></returns>
        public CS3 RelativeTo(CS3 parent)
        {
            return new CS3(parent, 
                parent.Global2Local(GlobalPosition), 
                parent.Global2Local(GlobalRotation));
        }

        public T FindPrevious<T>() where T : CS3
        {
            CS3 link = this;
            Type t = typeof(T);
            while( link.Parent!=null )
            {
                link = link.Parent;
                if(t.IsInstanceOfType(link))
                {
                    return link as T;
                }
            }
            return null;
        }

        public T[] FindNext<T>() where T : CS3
        {
            List<T> list = new List<T>();
            CS3 link = this;
            Type t = typeof(T);
            m_children.ForEach(delegate(CS3 child)
            {
                if(t.IsInstanceOfType(child))
                {
                    list.Add(child as T);
                }
                else
                {
                    list.AddRange(child.FindNext<T>());
                }
            });
            return list.ToArray();
        }

        /// <summary>
        /// Return the root coordinate system of which this is a branch of
        /// </summary>
        /// <returns>The root coordinate system</returns>
        public CS3 Root()
        {
            CS3 link = this;
            while( link.Parent!=null )
            {
                link = link.Parent;
            }
            return link;
        }

        public CS3 CreateZX(vec3 center, vec3 towards_Z, vec3 towards_X)
        {
            towards_Z -= center;
            towards_X -= center;
            return new CS3(this, center, Rotations.AlignZX(towards_Z, towards_X));
        }
        public CS3 CreateXY(vec3 center, vec3 towards_X, vec3 towards_Y)
        {
            towards_Y -= center;
            towards_X -= center;
            return new CS3(this, center, Rotations.AlignXY(towards_X, towards_Y));
        }
        public CS3 CreateYZ(vec3 center, vec3 towards_Y, vec3 towards_Z)
        {
            towards_Z -= center;
            towards_Y -= center;
            return new CS3(this, center, Rotations.AlignYZ(towards_Y, towards_Z));
        }

        public CS3 Translate(vec3 dr)
        {
            return new CS3(this, dr);
        }
        public CS3 Translate(num dx, num dy, num dz)
        {
            return new CS3(this, vec3.Cartesian(dx, dy, dz));
        }
        public CS3 Rotate(AxisName axis, num angle)
        {
            return new CS3(this, vec3.Zero, axis, angle);
        }
        public CS3 Rotate(vec3 axis, num angle)
        {
            return new CS3(this, vec3.Zero, axis, angle);
        }
        public CS3 Rotate(mat3 rotation)
        {
            return new CS3(this, rotation);
        }
        public CS3 Rotate(qua3 rotation)
        {
            return new CS3(this, rotation.ToRotationMatrix());
        }
        public CS3 TxRz(num del_x, num theta_z)
        {
            return new CS3(this, vec3.UnitX * del_x, Rotations.FromAxisRotation(AxisName.Z, theta_z));
        }
        #endregion

        #region Properties
        public int Level { get { return m_level; } }        
        public int ID { get { return m_id; } }
        public int ParentID { get { return Parent != null ? Parent.m_id : 0; } }
        public bool IsWorld { get { return Parent==null; } }
        public bool IsRoot { get { return Parent != null && Parent.Parent==null; } }
        public bool IsLeaf { get { return m_children.Count == 0; } }
        public CS3 Parent { get; set; }
        public string Name {
            get { return m_name ??string.Format("{0}({1})", this.GetType().Name, m_id); }
            set { m_name = value; }
        }
        public List<CS3> Children { get { return m_children; } }

        public vec3 GlobalPosition
        {
            get
            {
                return Parent != null ? Parent.Local2Global(Position) : Position;
            }
        }

        public mat3 GlobalRotation
        {
            get
            {
                return Parent != null ? Parent.Local2Global(Rotation) : Rotation;
            }
        }
        public qua3 GlobalOrientation
        {
            get
            {
                return Parent != null ? Parent.Local2Global(Orientation) : Orientation;
            }
        }
        #endregion

        #region Transforms Global to Local

        public vec3 Global2Local(vec3 point)
        {
            return Base2Local(Parent != null ? Parent.Global2Local(point) : point);
        }
        public vec3 Global2LocalDirection(vec3 direction)
        {
            return Base2LocalDirection(Parent != null ? Parent.Global2LocalDirection(direction) : direction);
        }
        public mat3 Global2Local(mat3 rotation)
        {
            return Base2Local(Parent != null ? Parent.Global2Local(rotation) : rotation);
        }
        public qua3 Global2Local(qua3 rotation)
        {
            return Base2Local(Parent != null ? Parent.Global2Local(rotation) : rotation);
        }

        #endregion

        #region Transforms Local to Global

        public vec3 Local2Global(vec3 point)
        {
            return Parent != null ? Parent.Local2Global(Local2Base(point)) : Local2Base(point);
        }
        public vec3 Local2GlobalDirection(vec3 direction)
        {
            return Parent != null ? Parent.Local2GlobalDirection(Local2BaseDirection(direction)) : Local2BaseDirection(direction);
        }
        public mat3 Local2Global(mat3 rotation)
        {
            return Parent != null ? Parent.Local2Global(Local2Base(rotation)) : Local2Base(rotation);
        }
        public qua3 Local2Global(qua3 rotation)
        {
            return Parent != null ? Parent.Local2Global(Local2Base(rotation)) : Local2Base(rotation);
        }
        #endregion

        #region Operators

        public static CS3 operator +(CS3 lhs, vec3 rhs)
        {
            return lhs.Translate(rhs);
        }
        public static CS3 operator *(CS3 lhs, mat3 rhs)
        {
            return lhs.Rotate(rhs);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("{0} @{1}", Name,  Position.ToString());
        }

        public TreeNode CreateTreeNode(TreeNodeCollection list)
        {
            TreeNode res = list.Add(string.Format("{0} Level={1}", Name, Level));
            res.Tag = this;
            AddTreeNodeProperties(res);
            m_children.ForEach(delegate(Coord3 child)
            {
                TreeNode sub = child.CreateTreeNode(res.Nodes);
            });
            return res;
        }

        protected virtual void AddTreeNodeProperties(TreeNode node)
        {
        }
    }
}
