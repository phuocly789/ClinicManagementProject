<?php

return [

    'paths' => ['api/*', 'oauth/*', 'sanctum/csrf-cookie'],

    'allowed_methods' => ['*'],

    'allowed_origins' => [
        'https://clinic.lmp.id.vn',
        'https://cliniclaravel.lmp.id.vn',
        // nếu có base domain
        'https://www.clinic.lmp.id.vn',
        // môi trường dev (nếu build từ local)
        'http://localhost:5173',
    ],

    'allowed_origins_patterns' => [],

    'allowed_headers' => ['*'],

    'supports_credentials' => true,

    'exposed_headers' => [],

    'max_age' => 0,
];
