import cv2
import numpy as np
import mss
from ultralytics import YOLO

# Load model
model = YOLO("my_model.pt")

# Define capture region
monitor = {"top": 300, "left": 100, "width": 1280, "height": 720}

sct = mss.mss()

while True:
    # Capture screen
    img = np.array(sct.grab(monitor))

    # Convert BGRA -> BGR (OpenCV format)
    frame = cv2.cvtColor(img, cv2.COLOR_BGRA2BGR)

    # Run inference
    results = model.predict(frame, conf=0.5)

    # Annotate frame
    annotated = results[0].plot()  # draws boxes, labels, etc.

    # Show
    cv2.imshow("YOLO Screen Detection", annotated)

    # Exit on 'q'
    if cv2.waitKey(1) & 0xFF == ord("q"):
        break

cv2.destroyAllWindows()
