// api:
// POST '/api/seed/generate'
//  GET '/api/seed/progress'
// POST '/api/seed/slow-queue
// POST '/api/seed/create-file

// POST '/api/seed/generate'
// Body: settingsString, seed
// Returns: id that the seed will use once it is created successfully.
//   The id is reserved while in the queue and once it is created.

// GET '/api/seed/progress/${id}'
// Checks if that id is in the queue's byId.
// If it is, returns its status, and any other relevant info
// like queue lengths and queue position if applicable.
// If doesn't exist, checks if the input.json file exists for that
// id.
// If it does, returns that it has already finished. UI may want to redirect
// at that point.
// If not in the queue and not on disk, returns that the id is not valid.
