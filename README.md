# ConceptMapper

A small tool for measuring concept maps. Developed to help research involving analyzing participants mental/conceptual models. Allows you to place and connect nodes on top of an imported image and then save the calculated data to a CSV file. A screenshot is also saved for future reference.

## Instructions
1) Place all concept map image files in a single folder with no other files.
2) Click "File→Select map image..." and navigate to the folder and select an image (or use "File→Select map image folder..."). Select the top file (alphabetically), or the file after the last completed image.
3) Click "File→Select output file..." and select a CSV file to save the information to. If the file does not exist, it will be created.
4) Use the mouse buttons to place nodes and connections to build the concept map.
5) Fill the fields that need to be manually filled.
6) Click "Done". This will export the information on the current image to a new line in the output file, then select the next image file in the same folder.

### Notes
- When no nodes are placed, left clicking will place the root node.
- The current node has a thick black outline (instead of red).
- Clicking the current node, or right clicking empty space will un-select the current node. When there is no current node, clicking on one will select it as current.
- Creating a new node can only be done when a current node is selected. The new node will have have a connection to the current one, then the new node will be selected as current.
- A new connection can be added to the current selected node by clicking another node. Right clicking will add a crosslink instead of an edge.
- The current selected node can be removed with the "Delete" or "Backspace" keys. Any nodes not connected to the root node will also be removed.
