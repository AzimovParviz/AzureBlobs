Minimal API that received a JSON with key, email and attributes (array of strings), e.g
{
    "Key": <string>,
    "Email":<string: email-address>
    "Attributes": ["one", "two"]
}
When the JSON is received, it logs the email and the JSON in the MySQL database and Azure Blob Storage