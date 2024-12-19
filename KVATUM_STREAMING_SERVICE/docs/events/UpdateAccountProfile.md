Event: `UpdateAccountProfile`

Body: `UpdateAccountProfileBody`

{
    "EventType": "UpdateAccountProfile",
    "EventBody": {
        "AccountProfile": {
            "Id": "123e4567-e89b-12d3-a456-426614174000",
            "Email": "john.doe@example.com",
            "Nickname": "John Doe",
            "Role": "User",
            "Images": [
                {
                    "Url": "https://example.com/image.jpg",
                    "Resolution": "Small"
                },

                {
                    "Url": "https://example.com/image.jpg",
                    "Resolution": "Medium"
                }
            ]
        }
    }
}