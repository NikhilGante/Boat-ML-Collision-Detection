import os
import json
import shutil

# Paths
root_dir = r"C:\Users\odyse\AppData\LocalLow\DefaultCompany\Lake (HDRP)\solo_14"  # Change to your main folder containing sequence.* subfolders
images_dir = os.path.join(root_dir, "images")
labels_dir = os.path.join(root_dir, "labels")
classes_file = os.path.join(root_dir, "classes.txt")

# Create output folders
os.makedirs(images_dir, exist_ok=True)
os.makedirs(labels_dir, exist_ok=True)

# Keep track of class names
class_names = []

# Loop through sequence folders
for seq_folder in os.listdir(root_dir):
    seq_path = os.path.join(root_dir, seq_folder)
    if not os.path.isdir(seq_path) or not seq_folder.startswith("sequence."):
        continue

    # Find .frame_data.json file
    for file in os.listdir(seq_path):
        if file.endswith(".frame_data.json"):
            json_path = os.path.join(seq_path, file)

            with open(json_path, "r") as f:
                data = json.load(f)

            capture = data["captures"][0]
            img_filename = capture["filename"]
            img_width, img_height = capture["dimension"]

            # Copy image to images/ folder
            src_img_path = os.path.join(seq_path, img_filename)
            dst_img_path = os.path.join(images_dir, img_filename)
            shutil.copy(src_img_path, dst_img_path)

            # Get bounding box annotations
            label_lines = []
            for ann in capture["annotations"]:
                if ann["@type"].endswith("BoundingBox2DAnnotation"):
                    for obj in ann["values"]:
                        label_name = obj["labelName"]
                        if label_name not in class_names:
                            class_names.append(label_name)
                        class_id = class_names.index(label_name)

                        # Convert from origin (top-left) to center
                        x_min, y_min = obj["origin"]
                        box_w, box_h = obj["dimension"]
                        x_center = (x_min + box_w / 2) / img_width
                        y_center = (y_min + box_h / 2) / img_height
                        w_norm = box_w / img_width
                        h_norm = box_h / img_height

                        label_lines.append(f"{class_id} {x_center:.6f} {y_center:.6f} {w_norm:.6f} {h_norm:.6f}")

            # Save YOLO label file
            label_filename = os.path.splitext(img_filename)[0] + ".txt"
            label_path = os.path.join(labels_dir, label_filename)
            with open(label_path, "w") as lf:
                lf.write("\n".join(label_lines))

# Save classes.txt
with open(classes_file, "w") as cf:
    cf.write("\n".join(class_names))

print(f"✅ Conversion complete. {len(class_names)} classes written to {classes_file}")
