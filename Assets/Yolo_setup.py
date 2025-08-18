import os
import json
import shutil
from glob import glob

# -------------------------
# CONFIG - set your path
# -------------------------
root_dir = r"C:\Users\odyse\AppData\LocalLow\DefaultCompany\Lake (HDRP)\solo_1"
images_dir = os.path.join(root_dir, "images")
labels_dir = os.path.join(root_dir, "labels")
classes_path = os.path.join(root_dir, "classes.txt")

os.makedirs(images_dir, exist_ok=True)
os.makedirs(labels_dir, exist_ok=True)

# -------------------------
# helpers
# -------------------------
def find_sequence_folders(base):
    # find directories named "sequence.*" directly under base
    seqs = []
    for name in os.listdir(base):
        p = os.path.join(base, name)
        if os.path.isdir(p) and name.startswith("sequence."):
            seqs.append(p)
    return seqs

def safe_copy(src, dst):
    if os.path.exists(src):
        shutil.copy2(src, dst)
        return True
    return False

# -------------------------
# main
# -------------------------
class_map = {}   # name -> id (0-indexed)
next_class_id = 0

sequence_folders = find_sequence_folders(root_dir)
if not sequence_folders:
    # fallback: maybe sequence folders are nested deeper, search recursively
    sequence_folders = [d for d in glob(os.path.join(root_dir, "**", "sequence.*"), recursive=True) if os.path.isdir(d)]

for seq_path in sorted(sequence_folders):
    seq_name = os.path.basename(seq_path)            # e.g. "sequence.0"
    # Find all .json files in this sequence folder (robust detection)
    for fname in os.listdir(seq_path):
        if not fname.lower().endswith(".json"):
            continue
        json_path = os.path.join(seq_path, fname)
        # Try to parse JSON - skip if not format we expect
        try:
            with open(json_path, "r", encoding="utf-8") as f:
                data = json.load(f)
        except Exception as e:
            print(f"Skipping {json_path}: failed to parse JSON ({e})")
            continue

        # basic sanity check
        if "captures" not in data or not data["captures"]:
            print(f"Skipping {json_path}: no 'captures' found")
            continue

        # pick RGB camera capture (search for RGBCamera or similar)
        capture = None
        for c in data["captures"]:
            t = c.get("@type", "").lower()
            if "rgb" in t or "camera" in t:
                capture = c
                break
        if capture is None:
            capture = data["captures"][0]

        # Extract identifying fields
        sequence_idx = data.get("sequence", None)
        frame_idx = data.get("frame", None)    # try frame number
        step_idx = data.get("step", None)      # fallback if needed

        # Compose unique base name: prefer sequence+frame; fallback to seq folder + json filename
        if sequence_idx is None or frame_idx is None:
            # fallback using seq folder and json file name
            base_name = f"{seq_name}_{os.path.splitext(fname)[0]}"
        else:
            base_name = f"seq{sequence_idx}_frame{int(frame_idx):06d}"

        # Image info from capture
        img_filename = capture.get("filename", None)
        dims = capture.get("dimension", [None, None])
        img_w = int(dims[0]) if dims and dims[0] is not None else None
        img_h = int(dims[1]) if dims and dims[1] is not None else None

        # Copy image into images_dir under new unique name
        if img_filename:
            src_img_path = os.path.join(seq_path, img_filename)
            # normalize extension to .png
            ext = os.path.splitext(img_filename)[1] or ".png"
            dest_img_name = base_name + ext
            dest_img_path = os.path.join(images_dir, dest_img_name)
            copied = safe_copy(src_img_path, dest_img_path)
            if not copied:
                # try also without spaces or with 'semantic' variants - but primarily warn
                print(f"Warning: image file not found: {src_img_path}")
        else:
            print(f"Warning: no filename in capture for {json_path}")
            continue

        # Prepare label lines
        label_lines = []

        # find 2D bounding box annotation block
        for ann in capture.get("annotations", []):
            t = ann.get("@type", "")
            if "BoundingBox2DAnnotation" in t or t.endswith("BoundingBox2DAnnotation"):
                for val in ann.get("values", []):
                    label_name = val.get("labelName", "unknown")
                    # add to class map
                    if label_name not in class_map:
                        class_map[label_name] = next_class_id
                        next_class_id += 1
                    class_id = class_map[label_name]

                    origin = val.get("origin", None)
                    dimension = val.get("dimension", None)
                    if origin is None or dimension is None:
                        continue
                    x_min, y_min = float(origin[0]), float(origin[1])
                    w_box, h_box = float(dimension[0]), float(dimension[1])

                    # Ensure we have image dims
                    if img_w is None or img_h is None:
                        print(f"Warning: missing image dims for {json_path}; skipping bbox.")
                        continue

                    # Convert top-left origin -> YOLO center format (normalized)
                    x_center = (x_min + w_box / 2.0) / img_w
                    y_center = (y_min + h_box / 2.0) / img_h
                    w_norm = w_box / img_w
                    h_norm = h_box / img_h

                    # clip to [0,1]
                    def clip01(v):
                        return max(0.0, min(1.0, v))

                    x_center = clip01(x_center)
                    y_center = clip01(y_center)
                    w_norm = clip01(w_norm)
                    h_norm = clip01(h_norm)

                    label_lines.append(f"{class_id} {x_center:.6f} {y_center:.6f} {w_norm:.6f} {h_norm:.6f}")

        # write label file (always create, even if empty)
        label_filename = base_name + ".txt"
        label_path = os.path.join(labels_dir, label_filename)
        with open(label_path, "w", encoding="utf-8") as lf:
            if label_lines:
                lf.write("\n".join(label_lines))
            else:
                # write empty file (YOLO frameworks typically accept empty label files)
                lf.write("")

print("Conversion finished. Writing classes.txt ...")

# Write classes.txt sorted by class id order
# Build reverse map of id -> name
id_to_name = {cid: name for name, cid in class_map.items()}
classes_ordered = [id_to_name[i] for i in sorted(id_to_name.keys())]

with open(classes_path, "w", encoding="utf-8") as cf:
    cf.write("\n".join(classes_ordered))

print(f"{len(classes_ordered)} classes written to {classes_path}")
print("Done.")
