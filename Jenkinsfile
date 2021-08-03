pipeline {
    agent any
    
    environment {
        scannerHome = tool name: 'sonar_scanner_dotnet'
        username = 'aakashgarg'
		    registry = 'iaakashgarg/app-test'
		    properties = null
		    docker_port = "${env.BRANCH_NAME == "master" ? "7200" : "7300"}"
    }
    
    tools {
		  msbuild 'MSBuild'
	  }

    
    options {
        timestamps()
        timeout(time: 1, unit: 'HOURS')
        buildDiscarder logRotator(artifactDaysToKeepStr: '', artifactNumToKeepStr: '', daysToKeepStr: '10', numToKeepStr: '20')
    }
    
    stages {
		
		stage('nuget restore') {
			steps {
				echo "Start restoring packages"
				bat "dotnet restore WebApplication4\\WebApplication4.csproj"
			}
		}
        
      stage('Start Sonarqube Analysis') {
			  when {
				  expression {
					  BRANCH_NAME == 'master'
				  }
			 }
        steps {
            echo "Start Sonarqube analysis"
               withSonarQubeEnv('Test_Sonar') {
                    bat "${scannerHome}\\SonarScanner.MSBuild.exe begin /k:sonar-aakashgarg /n:sonar-aakashgarg /v:1.0"
              }
            }
        }
        
        stage('Code build') {
            steps {
				// Cleans the output of a project
                echo "Clean previous build"
                bat "dotnet clean WebApplication4\\WebApplication4.csproj"
				
				// Builds the project and its dependencies
				echo "Start Building code"
				bat 'dotnet build WebApplication4\\WebApplication4.csproj -c Release -o "WebApplication4/app/build"'
            }
        }
		
		stage('Stop Sonarqube Analysis') {
			when {
				expression {
					BRANCH_NAME == 'master'
				}
			}
            steps {
                echo "Stop Sonarqube analysis"
                withSonarQubeEnv('Test_Sonar') {
                    bat "${scannerHome}\\SonarScanner.MSBuild.exe end "
                }
            }
        }
		
		stage('Release artifact') {
			when {
				expression {
					BRANCH_NAME == 'develop'
				}
			}
			steps {
				echo "Publish Code"
				bat "dotnet publish -c Release"
			}
		}
		
		stage('Docker Image') {
			steps {
				echo "Create Docker Image"
				bat "dotnet publish -c Release"
				bat "docker build -t i-${username}-${BRANCH_NAME} --no-cache -f Dockerfile ."
				bat "docker tag i-${username}-${BRANCH_NAME} ${registry}:${BUILD_NUMBER}"
			}
		}
		
		stage('Containers') {
			parallel {
				stage('PreContainerCheck') {
					steps {
						bat '''
						CONTAINER_ID = $(docker ps -a | grep ${docker_port} | cut -d " " -f 1)
						if [ $CONTAINER_ID ]
						then
							docker rm -f $CONTAINER_ID
						'''
					}
				}
				stage('PushtoDockerHub') {
					steps {
						echo "Push Image to Docker Hub"
				
						withDockerRegistry([credentialsId: 'DockerHub', url: ""]) {
							bat "docker push ${registry}:${BUILD_NUMBER}"
						}
					}
				}
			}
		}
		
		stage('Docker Deployment') {
			steps {
				echo "Docker Deployment"
				bat "docker run --name c-${username}-${BRANCH_NAME} -d -p ${docker_port}:80 ${registry}:${BUILD_NUMBER}"
			}
		}
		
		
	}

        
}
