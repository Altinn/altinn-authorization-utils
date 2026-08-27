namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Provides utility methods for retrieving standard descriptions for HTTP status codes.
/// </summary>
internal static class StatusDescriptions
{
    // Status descriptions for HTTP status codes. For most codes, this is the standard reason phrase.
    // Status Codes listed at http://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml
    private static readonly string?[][] HttpStatusDescriptions = [
        [],
        [
            /* 100 */ "Continue",
            /* 101 */ "Switching Protocols",
            /* 102 */ "Processing"
        ],
        [
            /* 200 */ "OK",
            /* 201 */ "Created",
            /* 202 */ "Accepted",
            /* 203 */ "Non-Authoritative Information",
            /* 204 */ "No Content",
            /* 205 */ "Reset Content",
            /* 206 */ "Partial Content",
            /* 207 */ "Multi-Status",
            /* 208 */ "Already Reported",
            /* 209 */ null,
            /* 210 */ null,
            /* 211 */ null,
            /* 212 */ null,
            /* 213 */ null,
            /* 214 */ null,
            /* 215 */ null,
            /* 216 */ null,
            /* 217 */ null,
            /* 218 */ null,
            /* 219 */ null,
            /* 220 */ null,
            /* 221 */ null,
            /* 222 */ null,
            /* 223 */ null,
            /* 224 */ null,
            /* 225 */ null,
            /* 226 */ "IM Used"
        ],
        [
            /* 300 */ "Multiple Choices",
            /* 301 */ "Moved Permanently",
            /* 302 */ "Found",
            /* 303 */ "See Other",
            /* 304 */ "Not Modified",
            /* 305 */ "Use Proxy",
            /* 306 */ "Switch Proxy",
            /* 307 */ "Temporary Redirect",
            /* 308 */ "Permanent Redirect"
        ],
        [
            /* 400 */ "Bad Request",
            /* 401 */ "Unauthorized",
            /* 402 */ "Payment Required",
            /* 403 */ "Forbidden",
            /* 404 */ "Not Found",
            /* 405 */ "Method Not Allowed",
            /* 406 */ "Not Acceptable",
            /* 407 */ "Proxy Authentication Required",
            /* 408 */ "Request Timeout",
            /* 409 */ "Conflict",
            /* 410 */ "Gone",
            /* 411 */ "Length Required",
            /* 412 */ "Precondition Failed",
            /* 413 */ "Content Too Large",
            /* 414 */ "URI Too Long",
            /* 415 */ "Unsupported Media Type",
            /* 416 */ "Range Not Satisfiable",
            /* 417 */ "Expectation Failed",
            /* 418 */ "I'm a teapot",
            /* 419 */ "Authentication Timeout",
            /* 420 */ null,
            /* 421 */ "Misdirected Request",
            /* 422 */ "Unprocessable Entity",
            /* 423 */ "Locked",
            /* 424 */ "Failed Dependency",
            /* 425 */ null,
            /* 426 */ "Upgrade Required",
            /* 427 */ null,
            /* 428 */ "Precondition Required",
            /* 429 */ "Too Many Requests",
            /* 430 */ null,
            /* 431 */ "Request Header Fields Too Large",
            /* 432 */ null,
            /* 433 */ null,
            /* 434 */ null,
            /* 435 */ null,
            /* 436 */ null,
            /* 437 */ null,
            /* 438 */ null,
            /* 439 */ null,
            /* 440 */ null,
            /* 441 */ null,
            /* 442 */ null,
            /* 443 */ null,
            /* 444 */ null,
            /* 445 */ null,
            /* 446 */ null,
            /* 447 */ null,
            /* 448 */ null,
            /* 449 */ null,
            /* 450 */ null,
            /* 451 */ "Unavailable For Legal Reasons",
            /* 452 */ null,
            /* 453 */ null,
            /* 454 */ null,
            /* 455 */ null,
            /* 456 */ null,
            /* 457 */ null,
            /* 458 */ null,
            /* 459 */ null,
            /* 460 */ null,
            /* 461 */ null,
            /* 462 */ null,
            /* 463 */ null,
            /* 464 */ null,
            /* 465 */ null,
            /* 466 */ null,
            /* 467 */ null,
            /* 468 */ null,
            /* 469 */ null,
            /* 470 */ null,
            /* 471 */ null,
            /* 472 */ null,
            /* 473 */ null,
            /* 474 */ null,
            /* 475 */ null,
            /* 476 */ null,
            /* 477 */ null,
            /* 478 */ null,
            /* 479 */ null,
            /* 480 */ null,
            /* 481 */ null,
            /* 482 */ null,
            /* 483 */ null,
            /* 484 */ null,
            /* 485 */ null,
            /* 486 */ null,
            /* 487 */ null,
            /* 488 */ null,
            /* 489 */ null,
            /* 490 */ null,
            /* 491 */ null,
            /* 492 */ null,
            /* 493 */ null,
            /* 494 */ null,
            /* 495 */ null,
            /* 496 */ null,
            /* 497 */ null,
            /* 498 */ null,
            /* 499 */ "Client Closed Request"
        ],
        [
            /* 500 */ "An error occurred while processing your request.",
            /* 501 */ "Not Implemented",
            /* 502 */ "Bad Gateway",
            /* 503 */ "Service Unavailable",
            /* 504 */ "Gateway Timeout",
            /* 505 */ "HTTP Version Not Supported",
            /* 506 */ "Variant Also Negotiates",
            /* 507 */ "Insufficient Storage",
            /* 508 */ "Loop Detected",
            /* 509 */ null,
            /* 510 */ "Not Extended",
            /* 511 */ "Network Authentication Required"
        ]
    ];

    public static string? GetStatusDescription(int? statusCode)
    {
        if (statusCode is null)
        {
            return null;
        }

        if ((uint)(statusCode - 100) < 500)
        {
            var (i, j) = Math.DivRem((uint)statusCode, 100);
            string?[] descriptions = HttpStatusDescriptions[i];
            if (j < (uint)descriptions.Length)
            {
                return descriptions[j];
            }
        }

        return null;
    }
}
