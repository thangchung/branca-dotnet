# Branca Tokens for .NET Core

Authenticated and encrypted API tokens using modern crypto.

[![Software License](https://img.shields.io/badge/license-MIT-brightgreen.svg?style=flat-square)](LICENSE.md)

## What?

[Branca](https://github.com/tuupola/branca-spec) is a secure easy to use token format which makes it hard to shoot yourself in the foot. It uses IETF XChaCha20-Poly1305 AEAD symmetric encryption to create encrypted and tamperproof tokens. Payload itself is an arbitrary sequence of bytes. You can use for example a JSON object, plain text string or even binary data serialized by [MessagePack](http://msgpack.org/) or [Protocol Buffers](https://developers.google.com/protocol-buffers/).

## Install

Install the library using .NET SDK.

```bash
$ dotnet restore
$ dotnet run
```

## Usage

Token payload can be any arbitrary data such as string containing an email
address. You also must provide a 32 byte secret key. The key is used for encrypting the payload.

TODO

You can keep the token size small by using a space efficient serialization method such as [MessagePack](http://msgpack.org/) or [Protocol Buffers](https://developers.google.com/protocol-buffers/).

TODO

## Timestamp

Branca token includes a timestamp when it was created. When decoding you can optionally pass a `ttl` parameter. Value is passed in seconds. Below example throws en exception if token is older than 60 minutes.

TODO

## Testing

You can run tests either manually or automatically on every code change.

```bash
$ dotnet test
```

## Contributing

Please see [CONTRIBUTING](CONTRIBUTING.md) for details.

## Security

If you discover any security related issues, please email tuupola@appelsiini.net instead of using the issue tracker.

## License

The MIT License (MIT). Please see [License File](LICENSE.md) for more information.
