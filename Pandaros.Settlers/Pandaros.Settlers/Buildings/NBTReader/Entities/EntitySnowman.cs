using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Buildings.NBTReader.Entities
{
    using Pandaros.Settlers.Buildings.NBTReader.Nbt;

    public class EntitySnowman : EntityMob
    {
        public static readonly SchemaNodeCompound SnowmanSchema = MobSchema.MergeInto(new SchemaNodeCompound("")
        {
            new SchemaNodeString("id", TypeId),
        });

        public static new string TypeId
        {
            get { return "SnowMan"; }
        }

        protected EntitySnowman (string id)
            : base(id)
        {
        }

        public EntitySnowman ()
            : this(TypeId)
        {
        }

        public EntitySnowman (TypedEntity e)
            : base(e)
        {
        }


        #region INBTObject<Entity> Members

        public override bool ValidateTree (TagNode tree)
        {
            return new NbtVerifier(tree, SnowmanSchema).Verify();
        }

        #endregion


        #region ICopyable<Entity> Members

        public override TypedEntity Copy ()
        {
            return new EntitySnowman(this);
        }

        #endregion
    }
}
