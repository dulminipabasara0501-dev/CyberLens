# CyberLens - build all six 3D models and export them as FBX files for Unity.
#
# HOW TO USE (Blender 5.2 LTS):
#   1. File > New > General.
#   2. Click the "Scripting" tab at the top of Blender.
#   3. Click "Open" in the text editor and choose this file.
#   4. Change EXPORT_FOLDER below to your Unity Models folder.
#   5. Click the Run (play) button, or press Alt+P with the mouse over the text editor.
#
# WARNING: this script first DELETES everything in the current Blender scene
# (including the default cube), so always run it in a new, empty file.
#
# All models are simple low-poly shapes (under 1,500 triangles in total),
# so they run smoothly on an Android phone.
# Blender axes: X = right, Y = depth, Z = up. The FRONT of every model faces -Y.

import os

import bmesh
import bpy
from mathutils import Matrix

# ---------------------------------------------------------------------------
# 1. CHANGE THIS LINE to the Models folder inside your Unity project.
#    Keep the r in front of the quotes. Example for Windows:
#    r"C:\Users\YourName\UnityProjects\CyberLens\Assets\CyberLens\Models"
# ---------------------------------------------------------------------------
EXPORT_FOLDER = r"C:\Users\YourName\UnityProjects\CyberLens\Assets\CyberLens\Models"

# Material names. Unity will use these same names to find its own materials.
MATERIAL_COLORS = {
    "M_Body": (0.106, 0.129, 0.314),     # dark navy-violet casing
    "M_Violet": (0.482, 0.247, 0.949),   # rich violet
    "M_Cyan": (0.0, 0.898, 1.0),         # cyan highlight
    "M_Magenta": (0.910, 0.231, 0.839),  # magenta accent
    "M_Screen": (0.02, 0.03, 0.09),      # dark screen / input fields
}


# ---------------------------------------------------------------------------
# Helper functions
# ---------------------------------------------------------------------------
def clear_scene():
    """Delete every object in the file so we start clean."""
    for obj in list(bpy.data.objects):
        bpy.data.objects.remove(obj, do_unlink=True)


def get_material(name):
    """Return a material with this name, creating it if needed."""
    material = bpy.data.materials.get(name)
    if material is None:
        material = bpy.data.materials.new(name)
    red, green, blue = MATERIAL_COLORS[name]
    material.diffuse_color = (red, green, blue, 1.0)
    return material


def make_root(name):
    """Create an empty parent object. Each model hangs under one root."""
    root = bpy.data.objects.new(name, None)
    bpy.context.scene.collection.objects.link(root)
    return root


def finish_part(parent, name, bm, location, material_name):
    """Turn a bmesh into a real object, parent it and give it a material."""
    mesh = bpy.data.meshes.new(name + "_Mesh")
    bm.to_mesh(mesh)
    bm.free()

    part = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(part)
    part.location = location
    part.parent = parent
    part.data.materials.append(get_material(material_name))
    return part


def add_box(parent, name, size, location, material_name):
    """A box. size = (width X, depth Y, height Z)."""
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.transform(
        bm,
        matrix=Matrix.Diagonal((size[0], size[1], size[2], 1.0)),
        verts=bm.verts,
    )
    return finish_part(parent, name, bm, location, material_name)


def add_cylinder(parent, name, radius, height, location, material_name, segments=24):
    """An upright cylinder. location is the CENTRE of the cylinder."""
    bm = bmesh.new()
    bmesh.ops.create_cone(
        bm,
        cap_ends=True,
        segments=segments,
        radius1=radius,
        radius2=radius,
        depth=height,
    )
    return finish_part(parent, name, bm, location, material_name)


def add_gem(parent, name, radius, location, material_name, detail=1):
    """A low-poly faceted sphere (looks like a gem)."""
    bm = bmesh.new()
    bmesh.ops.create_icosphere(bm, subdivisions=detail, radius=radius)
    return finish_part(parent, name, bm, location, material_name)


def add_shield(parent, name, scale, thickness, y_centre, z_centre, material_name):
    """A shield shape standing upright, facing -Y."""
    outline = [
        (-0.50, 0.70), (0.00, 0.82), (0.50, 0.70), (0.52, 0.20),
        (0.38, -0.25), (0.00, -0.70), (-0.38, -0.25), (-0.52, 0.20),
    ]
    half = thickness / 2.0
    bm = bmesh.new()

    front = [bm.verts.new((x * scale, y_centre - half, z * scale + z_centre)) for x, z in outline]
    back = [bm.verts.new((x * scale, y_centre + half, z * scale + z_centre)) for x, z in outline]

    bm.faces.new(front)
    bm.faces.new(list(reversed(back)))
    count = len(outline)
    for i in range(count):
        j = (i + 1) % count
        bm.faces.new((front[i], front[j], back[j], back[i]))

    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return finish_part(parent, name, bm, (0.0, 0.0, 0.0), material_name)


# ---------------------------------------------------------------------------
# The six models
# ---------------------------------------------------------------------------
def build_computer():
    root = make_root("Computer")
    add_box(root, "Monitor_Base", (0.50, 0.34, 0.05), (0.0, 0.0, 0.025), "M_Body")
    add_box(root, "Monitor_Neck", (0.12, 0.10, 0.35), (0.0, 0.04, 0.225), "M_Body")
    add_box(root, "Monitor_Body", (1.00, 0.08, 0.66), (0.0, 0.04, 0.73), "M_Body")
    add_box(root, "Screen_Display", (0.90, 0.02, 0.56), (0.0, -0.005, 0.73), "M_Screen")
    add_box(root, "Keyboard", (0.80, 0.26, 0.04), (0.0, -0.50, 0.02), "M_Violet")
    return root


def build_app_server():
    root = make_root("AppServer")
    add_box(root, "Server_Base", (0.78, 0.62, 0.06), (0.0, 0.0, 0.03), "M_Body")
    add_box(root, "Server_Body", (0.70, 0.55, 1.04), (0.0, 0.0, 0.58), "M_Body")
    for index, z in enumerate((0.28, 0.58, 0.88), start=1):
        add_box(root, "Server_Unit_" + str(index), (0.60, 0.03, 0.20), (0.0, -0.29, z), "M_Violet")
        add_box(root, "Server_Led_" + str(index), (0.05, 0.03, 0.05), (0.22, -0.31, z), "M_Cyan")
    return root


def build_database():
    root = make_root("Database")
    for index, z in enumerate((0.13, 0.45, 0.77), start=1):
        add_cylinder(root, "Disk_" + str(index), 0.45, 0.26, (0.0, 0.0, z), "M_Violet")
    for index, z in enumerate((0.29, 0.61), start=1):
        add_cylinder(root, "Ring_" + str(index), 0.46, 0.04, (0.0, 0.0, z), "M_Cyan")
    return root


def build_login_panel():
    root = make_root("LoginPanel")
    add_box(root, "Panel_Base", (0.70, 0.40, 0.05), (0.0, 0.0, 0.025), "M_Body")
    add_box(root, "Panel_Body", (0.90, 0.06, 1.20), (0.0, 0.0, 0.65), "M_Body")
    add_gem(root, "User_Icon", 0.10, (0.0, -0.04, 1.05), "M_Cyan", detail=2)
    add_box(root, "Field_Username", (0.70, 0.02, 0.12), (0.0, -0.04, 0.80), "M_Screen")
    add_box(root, "Field_Password", (0.70, 0.02, 0.12), (0.0, -0.04, 0.60), "M_Screen")
    add_box(root, "Login_Button", (0.40, 0.02, 0.14), (0.0, -0.04, 0.35), "M_Violet")
    return root


def build_data_token():
    root = make_root("DataToken")
    add_gem(root, "Token_Gem", 0.35, (0.0, 0.0, 0.40), "M_Cyan", detail=1)
    add_cylinder(root, "Token_Ring", 0.50, 0.03, (0.0, 0.0, 0.40), "M_Violet", segments=6)
    return root


def build_security_shield():
    root = make_root("SecurityShield")
    add_cylinder(root, "Shield_Pedestal", 0.38, 0.10, (0.0, 0.0, 0.05), "M_Body")
    add_shield(root, "Shield_Outer", 1.00, 0.12, 0.0, 0.80, "M_Violet")
    add_shield(root, "Shield_Inner", 0.78, 0.04, -0.07, 0.80, "M_Cyan")
    return root


# ---------------------------------------------------------------------------
# Export
# ---------------------------------------------------------------------------
def export_model(root, folder):
    """Export one model (the root and all its parts) as an FBX file."""
    for obj in bpy.context.scene.objects:
        obj.select_set(False)

    root.select_set(True)
    for child in root.children_recursive:
        child.select_set(True)
    bpy.context.view_layer.objects.active = root

    file_path = os.path.join(folder, root.name + ".fbx")
    bpy.ops.export_scene.fbx(
        filepath=file_path,
        use_selection=True,
        object_types={"EMPTY", "MESH"},
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        add_leaf_bones=False,
        mesh_smooth_type="FACE",
    )
    print("Exported: " + file_path)


def main():
    if not os.path.isdir(EXPORT_FOLDER):
        print("STOPPED: the folder does not exist: " + EXPORT_FOLDER)
        print("Fix the EXPORT_FOLDER line at the top of the script and run again.")
        return

    clear_scene()

    roots = [
        build_computer(),
        build_app_server(),
        build_database(),
        build_login_panel(),
        build_data_token(),
        build_security_shield(),
    ]

    failed = []
    for root in roots:
        try:
            export_model(root, EXPORT_FOLDER)
        except Exception as error:  # Show a clear message instead of stopping.
            failed.append(root.name)
            print("EXPORT FAILED for " + root.name + ": " + str(error))

    # Spread the models out so you can look at them side by side in Blender.
    # (This happens AFTER exporting, so the FBX files stay centred.)
    for index, root in enumerate(roots):
        root.location = ((index - 2.5) * 2.0, 0.0, 0.0)

    if failed:
        print("Some models were not exported: " + ", ".join(failed))
    else:
        print("DONE: 6 models exported to " + EXPORT_FOLDER)


main()
