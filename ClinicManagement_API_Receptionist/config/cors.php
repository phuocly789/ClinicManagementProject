<?php

return [
    'paths' => ['api/*', 'oauth/*', 'sanctum/csrf-cookie'],

    'allowed_methods' => ['*'],

    'allowed_origins' => [
        'https://clinic.lmp.id.vn',
        'https://cliniclaravel.lmp.id.vn',
        'http://localhost:3000',
    ],

    'allowed_origins_patterns' => [],

    'allowed_headers' => ['*'],

    'supports_credentials' => true,

    'exposed_headers' => [],

    'max_age' => 0,
];
