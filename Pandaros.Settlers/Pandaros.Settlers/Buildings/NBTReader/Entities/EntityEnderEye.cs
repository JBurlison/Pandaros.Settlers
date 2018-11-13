using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Buildings.NBTReader.Entities
{
    using Pandaros.Settlers.Buildings.NBTReader.Nbt;

    public class EntityEnderEye : TypedEntity
    {
        public static readonly SchemaNodeCompound EnderEyeSchema = TypedEntity.Schema.MergeInto(new SchemaNodeCompound("")
        {
            new SchemaNodeString("id", TypeId),
        });

        public static string TypeId
        {
            get { return "EyeOfEnderSignal"; }
        }

        protected EntityEnderEye (string id)
            : base(id)
        {
        }

        public EntityEnderEye ()
            : this(TypeId)
        {
        }

        public EntityEnderEye (TypedEntity e)
            : base(e)
        {
        }


        #region INBTObject<Entity> Members

        public override bool ValidateTree (TagNode tree)
        {
            return new NbtVerifier(tree, EnderEyeSchema).Verify();
        }

        #endregion


        #region ICopyable<Entity> Members

        public override TypedEntity Copy ()
        {
            return new EntityEnderEye(this);
        }

        #endregion
    }
}
