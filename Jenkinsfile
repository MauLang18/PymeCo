pipeline {
    agent any

    options {
        skipDefaultCheckout(true)
    }

    environment {
        DOCKER_IMAGE = 'maulang18/pymeco:latest'
        CONTAINER_NAME_DEV = 'PymeCoCC'
        PORT_DEV = '10122'
        PORT_CONTAINER = '80'
        COMPOSE_NAME = '/home/administrador/docker-compose-customcode.yml'
        DOCKER_CREDENTIALS_ID = 'dockerhub-credentials-id'
    }

    stages {

        stage('Clean Workspace') {
            steps {
                echo 'Cleaning Jenkins workspace...'
                deleteDir()
            }
        }

        stage('Checkout SCM') {
            steps {
                checkout scm
            }
        }

        stage('Check Dev Container Running') {
            steps {
                script {
                    def running = sh(
                        script: "docker ps -q -f name=${CONTAINER_NAME_DEV}",
                        returnStdout: true
                    ).trim()

                    env.DEV_CONTAINER_RUNNING = running ? 'true' : 'false'
                }
            }
        }

        stage('Docker Build') {
            when {
                expression { env.DEV_CONTAINER_RUNNING == 'false' }
            }
            steps {
                echo 'Building Docker image (no cache)...'
                sh "docker build --no-cache -t ${DOCKER_IMAGE} ."
            }
        }

        stage('Docker Run (Development)') {
            when {
                expression { env.GIT_BRANCH == 'origin/development' }
            }
            steps {
                script {
                    if (env.DEV_CONTAINER_RUNNING == 'true') {
                        echo 'Development container already running.'
                    } else {
                        sh """
                          docker run -d \
                            -p ${PORT_DEV}:${PORT_CONTAINER} \
                            --name ${CONTAINER_NAME_DEV} \
                            ${DOCKER_IMAGE}
                        """
                    }
                }
            }
        }

        stage('Docker Compose Up (Production)') {
            when {
                expression { env.GIT_BRANCH == 'origin/master' }
            }
            steps {
                script {
                    sh "docker stop ${CONTAINER_NAME_DEV} || true"
                    sh "docker rm ${CONTAINER_NAME_DEV} || true"

                    dir('/home/administrador') {
                        sh "docker-compose -f ${COMPOSE_NAME} up -d"
                    }
                }
            }
        }
    }

    post {
        always {
            echo 'Cleaning unused Docker images...'
            sh "docker image prune -f"
        }

        success {
            echo 'Pipeline succeeded!'
            withCredentials([
                usernamePassword(
                    credentialsId: DOCKER_CREDENTIALS_ID,
                    usernameVariable: 'DOCKER_USER',
                    passwordVariable: 'DOCKER_PASS'
                )
            ]) {
                sh "docker login -u ${DOCKER_USER} -p ${DOCKER_PASS}"
                sh "docker push ${DOCKER_IMAGE}"
            }
        }

        failure {
            echo 'Pipeline failed!'
        }
    }
}

